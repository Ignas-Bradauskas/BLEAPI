using MongoDB.Bson;
using MongoDB.Driver;

namespace BLEBackend.Services
{
    public static class DevicePersistenceService
    {
        public static IMongoCollection<BsonDocument> MongoCollection { get; set; }
        public static IMongoDatabase MongoDatabase { get; set; }
        public static void Initialize()
        {
            // Prepare connection.
            var client = new MongoClient(
                "mongodb://ignas:LMdRKuJc1GLt6eOsJUb6AduWnh7jR1WmMLVMwnGMS65wMIJQ5uAgUiaRdW04YmFzH7Afy96l14NaHDWTKIf7pA==@ignas.documents.azure.com:10255/?ssl=true&replicaSet=globaldb"
            );
            var db = client.GetDatabase("test");
            MongoDatabase = db;
        }


    }
}
