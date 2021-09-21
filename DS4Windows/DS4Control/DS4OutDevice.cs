using System.Collections.Generic;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;

namespace DS4Windows
{
    internal abstract class DS4OutDevice : OutputDevice
    {
        public const string devtype = "DS4";

        public IDualShock4Controller cont;

        //public DualShock4FeedbackReceivedEventHandler forceFeedbackCall;
        public Dictionary<int, DualShock4FeedbackReceivedEventHandler> forceFeedbacksDict =
            new();

        protected DS4OutDevice(ViGEmClient client)
        {
            cont = client.CreateDualShock4Controller();
            //cont = client.CreateDualShock4Controller(0x054C, 0x09CC);
            cont.AutoSubmitReport = false;
        }

        public override void Connect()
        {
            cont.Connect();
            connected = true;
        }

        public override void Disconnect()
        {
            foreach (var pair in forceFeedbacksDict) cont.FeedbackReceived -= pair.Value;

            forceFeedbacksDict.Clear();

            connected = false;
            cont.Disconnect();
            //cont.Dispose();
            cont = null;
        }

        public override string GetDeviceType()
        {
            return devtype;
        }

        public override void RemoveFeedbacks()
        {
            foreach (var pair in forceFeedbacksDict) cont.FeedbackReceived -= pair.Value;

            forceFeedbacksDict.Clear();
        }

        public override void RemoveFeedback(int inIdx)
        {
            if (forceFeedbacksDict.TryGetValue(inIdx, out var handler))
            {
                cont.FeedbackReceived -= handler;
                forceFeedbacksDict.Remove(inIdx);
            }
        }
    }
}