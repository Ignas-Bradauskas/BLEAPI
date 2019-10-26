using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace BLEConsoleApp
{
    public static class DeviceLocatorService
    {
        private static readonly Dictionary<ulong, BluetoothDevice> DevicesFoundDictionary = new Dictionary<ulong, BluetoothDevice>();
        private static readonly object LockToken = new object();
        private static readonly BluetoothLEAdvertisementWatcher Watcher = new BluetoothLEAdvertisementWatcher();


        public static void Initialize()
        {
            Watcher.ScanningMode = BluetoothLEScanningMode.Active;
            Watcher.Received += DeviceFound;
            Watcher.Start();
        }

        public static Dictionary<ulong, BluetoothDevice> Poll()
        {
            var a = new Dictionary<ulong, BluetoothDevice>();
            lock (LockToken)
            {
                var temp = new Dictionary<ulong, BluetoothDevice>();
                foreach (var bluetoothDevice in DevicesFoundDictionary)
                {
                    temp.Add(bluetoothDevice.Key, bluetoothDevice.Value);
                }
                a = temp;
            }
            return a;
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
