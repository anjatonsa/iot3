from flask import Flask
from  nats.aio.client import Client as natsClient
import paho.mqtt.client as mqtt
import json
import threading
import time
import asyncio
from datetime import datetime,timedelta
import numpy as np


app = Flask(__name__)

broker_address = "mosquitto"
broker_port = 1883
sub_topic = "Sensor data"
data_window = []
window_size = 10 #sec

global first_in_window

lock = threading.Lock()

nats_url="nats://nats-server:4222"
nats_topic="averages"


def on_connect(client, userdata, flags, rc):
    if rc ==0:
        print("Connected to MQTT broker with result code " + str(rc))
        client.subscribe(sub_topic, qos=0)
    else:
        print("Connection to MQTT broker unsuccessful.")


def on_message(client, userdata, msg):
    message_data = json.loads(msg.payload.decode())
    print(f"Received message from topic {msg.topic}, {message_data}")
    process_messages(message_data)


def process_messages(msg):
    global data_window, first_in_window
    if len(data_window)==0:
        first_in_window = msg
        data_window.append(msg)
    else:
        if (datetime.fromisoformat(msg['Timestamp'].rstrip('Z')) - datetime.fromisoformat(first_in_window['Timestamp'].rstrip('Z')) ).total_seconds() < window_size:
            data_window.append(msg)
        else:
            avg_temperature  = np.mean([data["Temperature"] for data in data_window])
            avg_humidity  = np.mean([data["Humidity"] for data in data_window])
            avg_tvoc  = np.mean([data["TVOC"] for data in data_window])
            avg_eco2  = np.mean([data["eCO2"] for data in data_window])
            avg_rawh2 = np.mean([data["RawH2"] for data in data_window])
            avg_rawethanol  = np.mean([data["RawEthanol"] for data in data_window])
            avg_pressure  = np.mean([data["Pressure"] for data in data_window])
            avg_pm10 = np.mean([data["PM10"] for data in data_window])
            avg_pm25  = np.mean([data["PM25"] for data in data_window])
            avg_nc05  = np.mean([data["NC05"] for data in data_window])
            avg_nc10  = np.mean([data["NC10"] for data in data_window])
            avg_nc25  = np.mean([data["NC25"] for data in data_window])
            avg_firealarm = (int)(np.mean([data["FireAlarm"] for data in data_window]) >=0.5)

            avg_data={
                "avg_temperature":avg_temperature,
                "avg_humidity": avg_humidity,
                "avg_tvoc": avg_tvoc,
                "avg_eco2": avg_eco2,
                "avg_rawh2": avg_rawh2,
                "avg_rawethanol": avg_rawethanol,
                "avg_pressure": avg_pressure,
                "avg_pm10": avg_pm10,
                "avg_pm25": avg_pm25,
                "avg_nc05": avg_nc05,
                "avg_nc10": avg_nc10,
                "avg_nc25": avg_nc25,
                "avg_firealarm": avg_firealarm
            }
            print(f"Average data {avg_data} for publishing to NATS.")
            asyncio.run(publish_average_data(avg_data))
            data_window.clear()


async def publish_average_data(average_data):

    try:
        nc = natsClient()
        await nc.connect(servers=[nats_url])
        print("Connected to NATS server")
        message = json.dumps(average_data)
        await nc.publish(nats_topic, message.encode('utf-8'))
        await nc.drain()
        print("Published data to NATS")
    except Exception as e:
        print(f"Failed to publish data to NATS: {e}")


@app.route('/')
def index():
    return 'Filter  microservice'


if __name__ == '__main__':

    client = mqtt.Client()
    client.on_connect = on_connect
    client.on_message = on_message

    client.connect(broker_address, broker_port, 60)
    client.loop_start()
    app.run()
