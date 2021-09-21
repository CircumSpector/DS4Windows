using System.Collections.Generic;
using System.Threading;

namespace DS4Windows
{
    public class ControllerSlotManager
    {
        public ControllerSlotManager()
        {
            ControllerColl = new List<DS4Device>();
            ControllerDict = new Dictionary<int, DS4Device>();
            ReverseControllerDict = new Dictionary<DS4Device, int>();
        }

        public ReaderWriterLockSlim CollectionLocker { get; } = new();

        public List<DS4Device> ControllerColl { get; set; }

        public Dictionary<int, DS4Device> ControllerDict { get; }

        public Dictionary<DS4Device, int> ReverseControllerDict { get; }

        public void AddController(DS4Device device, int slotIdx)
        {
            using (var locker = new WriteLocker(CollectionLocker))
            {
                ControllerColl.Add(device);
                ControllerDict.Add(slotIdx, device);
                ReverseControllerDict.Add(device, slotIdx);
            }
        }

        public void RemoveController(DS4Device device, int slotIdx)
        {
            using (var locker = new WriteLocker(CollectionLocker))
            {
                ControllerColl.Remove(device);
                ControllerDict.Remove(slotIdx);
                ReverseControllerDict.Remove(device);
            }
        }

        public void ClearControllerList()
        {
            using (var locker = new WriteLocker(CollectionLocker))
            {
                ControllerColl.Clear();
                ControllerDict.Clear();
                ReverseControllerDict.Clear();
            }
        }
    }
}