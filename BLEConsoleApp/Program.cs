using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Microsoft.AspNetCore.Hosting;
using Nancy;
using Nancy.ModelBinding;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BLEConsoleApp
{
    class Program
    {
        private static readonly Dictionary<ulong, BluetoothDevice> DevicesFoundDictionary = new Dictionary<ulong, BluetoothDevice>();
        private static readonly object LockToken = new object();

        static async Task Main(string[] args)
        {
            var client = new MongoClient(
                "mongodb://ignas:LMdRKuJc1GLt6eOsJUb6AduWnh7jR1WmMLVMwnGMS65wMIJQ5uAgUiaRdW04YmFzH7Afy96l14NaHDWTKIf7pA==@ignas.documents.azure.com:10255/?ssl=true&replicaSet=globaldb"
            );

            var db = client.GetDatabase("test");
            var collection = db.GetCollection<BsonDocument>("devices");

            var watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Passive
            };
            watcher.Received += DeviceFound;
            watcher.Start();

            // Displaying devices
            var sb = new StringBuilder();
            while (true)
            {
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("Found signals");
                Console.WriteLine("Address\t\tSignalStrength\t\tLastSeen");
                var listToWriteToDb = new List<BsonDocument>();
                var jsonToConverToBson = "";

                lock (LockToken)
                {
                    foreach (var bluetoothDevice in DevicesFoundDictionary)
                    {
                        sb.AppendLine($"{bluetoothDevice.Key}\t\t{bluetoothDevice.Value.SignalStrength}\t\t{bluetoothDevice.Value.LastSeen}\t\t{bluetoothDevice.Value.Name}");
                        jsonToConverToBson = Newtonsoft.Json.JsonConvert.SerializeObject(bluetoothDevice);
                        var bsonDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonToConverToBson);
                        listToWriteToDb.Add(bsonDoc);
                    }
                }

                foreach (var doc in listToWriteToDb)
                {
                    await collection.InsertOneAsync(doc);
                }

                Console.WriteLine(sb.ToString());
                sb.Clear();
                listToWriteToDb.Clear();

            }
        }

        private static void DeviceFound(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            lock (LockToken)
            {
                DevicesFoundDictionary[args.BluetoothAddress] = new BluetoothDevice(args.RawSignalStrengthInDBm, args.Timestamp, args.Advertisement.LocalName);
            }
        }
    }
}
