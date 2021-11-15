using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace BLEConsoleApp
{
    class Program
    {
        private static readonly Dictionary<ulong, BluetoothDevice> DevicesFoundDictionary = new Dictionary<ulong, BluetoothDevice>();
        private static readonly object LockToken = new object();

        static async Task Main(string[] args)
        {
            var watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Passive
            };
            watcher.Received += DeviceFound;
            watcher.Start();

            while (true)
            {
                Thread.Sleep(1000);

                lock (LockToken)
                {
                    Console.Clear();
                    var table = new ConsoleTable("ID", "Name", "Signal strength", "Last seen");
                    foreach (var bluetoothDevice in DevicesFoundDictionary)
                    {
                        table.AddRow(bluetoothDevice.Key, bluetoothDevice.Value.Name, bluetoothDevice.Value.SignalStrength, bluetoothDevice.Value.LastSeen);
                    }
                    table.Write();
                }
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
