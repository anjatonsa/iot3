using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MQTTnet;
using MQTTnet.Client;
using SensorMicroservice.Models;
using SensorMicroservice.Services;

class Sensor
{
    private readonly int retryCount;
    private int retryAttempts;
    private int retryAttempts2;
    private readonly TimeSpan retryDelay;
    private readonly MqttFactory factory;
    private readonly IMqttClient client;
    private MqttClientOptions options;

    public Sensor()
    {
        Console.WriteLine($"Sensor microservice started...");
        this.retryCount = 3;        
        this.retryDelay = TimeSpan.FromSeconds(15); 
        this.retryAttempts = 0;
        this.retryAttempts2 = 0;
        this.factory = new MqttFactory();
        this.client = factory.CreateMqttClient();
    }

    private async Task ConnectToMqtt()
    {
        do
        {
            this.options = new MqttClientOptionsBuilder()
                .WithTcpServer("mosquitto", 1883)
                .Build();

            try
            {
                await client.ConnectAsync(options);
                Console.WriteLine($"Connected to MQTT.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection attempt failed. Retrying... ({ex.Message})");
                retryAttempts++;
                await Task.Delay(retryDelay);
            }
        } while (!client.IsConnected && retryAttempts < retryCount);

    }

    private async Task SendMessageToMqttTopic(string topic, Measurement message)
    {

        if (client.IsConnected)
        {
            string mess = JsonSerializer.Serialize(message);

            var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(mess)
                    .Build();

              await client.PublishAsync(applicationMessage, CancellationToken.None);

              Console.WriteLine($"Data published to MQTT broker to topic {topic}.");
           
        }
        else
        {
            Console.WriteLine("Failed to send data to MQTT broker, client is not connected.");
        }
    }

    private async Task<MeasurementDataAccess> ConnectToDB(MongoDbConfiguration conf)
    {
        Boolean connected = false;
        MeasurementDataAccess measurementDataAccess = null;

        do
        {
            try
            {
                measurementDataAccess = new MeasurementDataAccess(conf.ConnectionString, conf.DatabaseName, conf.CollectionName);
                connected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connecting to database failed. Retrying... ({ex.Message})");
                retryAttempts2++;
                await Task.Delay(retryDelay);
            }
        } while (!connected && retryAttempts2 < retryCount);

        return measurementDataAccess;

    }

    public async Task Start(MongoDbConfiguration conf)
    {
        await ConnectToMqtt();

        MeasurementDataAccess measurementDataAccess = await ConnectToDB(conf);


        var cursor = await measurementDataAccess.GetCursorAsync();

        while (await cursor.MoveNextAsync())
        {
            var batch = cursor.Current;

            foreach (var measurement in batch)
            {
                await SendMessageToMqttTopic("Sensor data", measurement);
                await Task.Delay(1000);
            }
        }

    }
}
