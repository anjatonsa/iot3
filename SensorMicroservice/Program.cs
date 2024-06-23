using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Configuration;
using MQTTnet;
using MQTTnet.Client;
using SensorMicroservice.Models;
using SensorMicroservice.Services;
class Program
{
    static async Task Main(string[] args)
    {

        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();

        string connectionString = configuration["MongoDbConfiguration:ConnectionString"];
        string databaseName = configuration["MongoDbConfiguration:DatabaseName"];
        string collectionName = configuration["MongoDbConfiguration:CollectionName"];

        MongoDbConfiguration conf = new MongoDbConfiguration(connectionString, databaseName, collectionName);


        Sensor sensor = new Sensor();
        await sensor.Start(conf);

    }

}
