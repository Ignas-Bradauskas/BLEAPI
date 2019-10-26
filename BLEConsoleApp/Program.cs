using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace BLEConsoleApp
{
    class Program
    {
        private static readonly Dictionary<ulong, BluetoothDevice> DevicesFoundDictionary = new Dictionary<ulong, BluetoothDevice>();
        private static readonly object LockToken = new object();
        private static int HubId = 0;

        static async Task Main(string[] args)
        {
            // Read config.
            var config = JObject.Parse(File.ReadAllText("Properties/Config.txt"));
            HubId = config["HubId"].ToObject<int>();

            // Prepare connection.
            var client = new MongoClient(
                "mongodb://ignas:LMdRKuJc1GLt6eOsJUb6AduWnh7jR1WmMLVMwnGMS65wMIJQ5uAgUiaRdW04YmFzH7Afy96l14NaHDWTKIf7pA==@ignas.documents.azure.com:10255/?ssl=true&replicaSet=globaldb"
            );
            var db = client.GetDatabase("test");
            var collection = db.GetCollection<BsonDocument>("hub"+HubId);

            var watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Passive
            };
            watcher.Received += DeviceFound;
            watcher.Start();

            // Inserting to db.
            while (true)
            {
                Thread.Sleep(1000);
                var listToWriteToDb = new List<BsonDocument>();
                var jsonToConvertToBson = "";

                lock (LockToken)
                {
                    foreach (var bluetoothDevice in DevicesFoundDictionary)
                    {
                        jsonToConvertToBson = Newtonsoft.Json.JsonConvert.SerializeObject(bluetoothDevice);
                        var bsonDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonToConvertToBson);
                        bsonDoc["_id"] = bsonDoc["Key"];
                        bsonDoc["HubId"] = HubId;
                        listToWriteToDb.Add(bsonDoc);
                        Console.WriteLine(bluetoothDevice.Key);
                    }
                }

                foreach (var doc in listToWriteToDb)
                {
                    await collection.DeleteOneAsync(filter: new BsonDocument("_id", doc["Key"]));
                    await collection.InsertOneAsync(doc);
                }
                listToWriteToDb.Clear();
            }
        }

        private static void DeviceFound(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            lock (LockToken)
            {
                DevicesFoundDictionary[args.BluetoothAddress] = new BluetoothDevice(args.RawSignalStrengthInDBm, args.Timestamp.ToUniversalTime(), args.Advertisement.LocalName);
            }
        }
    }
}
