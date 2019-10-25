using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Windows.Devices.Bluetooth.Advertisement;

namespace BLEConsoleApp
{
    class Program
    {
        private static readonly Dictionary<ulong, BluetoothDevice> DevicesFoundDictionary = new Dictionary<ulong, BluetoothDevice>();
        private static readonly object LockToken = new object();

        static void Main(string[] args)
        {

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
                lock (LockToken)
                {
                    foreach (var bluetoothDevice in DevicesFoundDictionary)
                    {
                        sb.AppendLine($"{bluetoothDevice.Key}\t\t{bluetoothDevice.Value.SignalStrength}\t\t{bluetoothDevice.Value.LastSeen}\t\t{bluetoothDevice.Value.Name}");
                    }
                }
                Console.WriteLine(sb.ToString());
                sb.Clear();
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
