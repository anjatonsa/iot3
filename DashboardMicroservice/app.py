import os
from flask import Flask
from  nats.aio.client import Client as natsClient
import asyncio
from datetime import datetime
from influxdb_client import InfluxDBClient, Point
from influxdb_client.client.write_api import SYNCHRONOUS
from dotenv import load_dotenv
import json


app = Flask(__name__)

nats_url="nats://nats-server:4222"
nats_topic="averages"

load_dotenv()
url = os.getenv('URL', None)
org = os.getenv('ORG', None)
token = os.getenv('TOKEN',None)
bucket = os.getenv('BUCKET', None)

print(url, org, bucket, token)


@app.route('/')
def index():
    return 'Dashboard microservice'


async def nats_subscriber():
    nc = natsClient()
    client = InfluxDBClient(url=url, token=token, org=org)
    write_api = client.write_api(write_options=SYNCHRONOUS)


    async def message_handler(msg):
        nonlocal write_api
        data = msg.data.decode()
        data = json.loads(data)
        print(f"NATS - Received a message: {data}")

        try:
            point = Point("sensor_data") \
                .field("avg_temperature", data['avg_temperature']) \
                .field("avg_humidity",data['avg_humidity']).field("avg_tvoc",data['avg_tvoc']) \
                .field("avg_eco2",data['avg_eco2']).field("avg_rawh2",data['avg_rawh2']) \
                .field("avg_rawethanol",data['avg_rawethanol']).field("avg_pressure",data['avg_pressure']) \
                .field("avg_pm10",data['avg_pm10']).field("avg_pm25",data['avg_pm25']) \
                .field("avg_nc05",data['avg_nc05']).field("avg_nc10",data['avg_nc10']) \
                .field("avg_nc25",data['avg_nc25']).field("avg_firealarm",data['avg_firealarm']) \
                .time(datetime.utcnow().isoformat())
            write_api.write(bucket, org, point)
        except Exception as e:
            print(f"Error storing data in InfluxDB: {e}")


    await nc.connect(servers=[nats_url])
    await nc.subscribe(nats_topic, cb=message_handler)
    print(f"Subscribed to NATS topic '{nats_topic}'")

    while True:
        await asyncio.sleep(1)


def start_nats_subscriber():
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    loop.run_until_complete(nats_subscriber())
    loop.run_forever()



if __name__ == '__main__':

    start_nats_subscriber()
    app.run()
