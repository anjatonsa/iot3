import pandas as pd
from pymongo import MongoClient
from datetime import datetime

client = MongoClient('mongodb://localhost:27017')
db = client['smoke_detection']
collection = db['sensor_data']


df = pd.read_csv('smoke_detection_iot.csv')

df['Timestamp'] = pd.to_datetime(df['UTC'], unit='s')
df['Fire Alarm'] = df['Fire Alarm'].astype(bool)

df.rename(columns={'Temperature[C]': 'Temperature'}, inplace=True)
df.rename(columns={'Humidity[%]': 'Humidity'}, inplace=True)
df.rename(columns={'TVOC[ppb]': 'TVOC'}, inplace=True)
df.rename(columns={'eCO2[ppm]': 'eCO2'}, inplace=True)
df.rename(columns={'Pressure[hPa]': 'Pressure'}, inplace=True)


df = df.drop(columns=['UTC'])
df = df.drop(columns=['CNT'])


data = df.to_dict(orient='records')
collection.insert_many(data)

print("Data inserted successfully!")
