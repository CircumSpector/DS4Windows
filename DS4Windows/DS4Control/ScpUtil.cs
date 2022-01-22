using System;
using System.Net.NetworkInformation;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows
{
    public class SerialChangeArgs : EventArgs
    {
        private int Index { get; }
        private PhysicalAddress Serial { get; }

        public SerialChangeArgs(int index, PhysicalAddress serial)
        {
            Index = index;
            Serial = serial;
        }
    }

    

    
}
