using System;

namespace BLEConsoleApp
{
    class BluetoothDevice
    {
        public short SignalStrength { get; set; }

        public DateTimeOffset LastSeen { get; set; }

        public string Name { get; set; }

        public BluetoothDevice(short signalStrength, DateTimeOffset lastSeen, string name)
        {
            SignalStrength = signalStrength;
            LastSeen = lastSeen;
            Name = name;
        }
    }
}
