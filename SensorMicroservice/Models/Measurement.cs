using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace SensorMicroservice.Models
{
    public class Measurement
    {
        [JsonIgnore]
        public ObjectId id { get; set; }

        [JsonIgnore]
        public int UID { get; set; }

        public double Temperature { get; set; } //Air temeperature

        public double Humidity { get; set; } //Air Humidity

        public int TVOC { get; set; } //Total Volatile Organic Compounds; measured in parts per billion


        [BsonElement("eCO2")]
        public double eCO2 { get; set; }  //CO2 equivalent concentration in ppm


        [BsonElement("Raw H2")]
        public double RawH2 { get; set; } //raw molecular hydrogen


        [BsonElement("Raw Ethanol")]
        public double RawEthanol { get; set; } //Raw ethanol gas

        public double Pressure { get; set; }  //Air pressure


        [BsonElement("PM1.0")]
        public double PM10{ get; set; }   //particulate matter size < 1.0 µm (PM1.0)


        [BsonElement("PM2.5")]  
        public double PM25{ get; set; }  // particulate matter size  between 1.0 µm and 2.5 µm (PM2.5)


        [BsonElement("NC0.5")]
        public double NC05{ get; set; }  //Number concentration of particulate matter. This differs from PM because NC gives the actual number of particles in the air.
                                         //The raw NC is also classified by the particle size:
                                         //< 0.5 µm (NC0.5); 0.5 µm < 1.0 µm (NC1.0); 1.0 µm < 2.5 µm (NC2.5);

        [BsonElement("NC1.0")]
        public double NC10{ get; set; }


        [BsonElement("NC2.5")]
        public double NC25{ get; set; }


        [BsonElement("Fire Alarm")]
        public bool FireAlarm { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
