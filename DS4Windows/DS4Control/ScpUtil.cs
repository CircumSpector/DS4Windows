using System;
using System.Net.NetworkInformation;

namespace DS4Windows
{
    public class SerialChangeArgs : EventArgs
    {
        public SerialChangeArgs(int index, PhysicalAddress serial)
        {
            Index = index;
            Serial = serial;
        }

        private int Index { get; }
        private PhysicalAddress Serial { get; }
    }
}