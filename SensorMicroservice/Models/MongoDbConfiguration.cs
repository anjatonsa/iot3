namespace SensorMicroservice.Models
{
    public class MongoDbConfiguration
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; } = null!;

        public MongoDbConfiguration(string ConnectionString, string DatabaseName, string CollectionName) {
            
            this.ConnectionString = ConnectionString;
            this.DatabaseName = DatabaseName;
            this.CollectionName = CollectionName;
        }
    }
}
