using DS4Windows.Shared.Common.Legacy;

namespace DS4Windows.Shared.Emulator.ViGEmGen1.Types.Legacy
{
    public abstract class OutputDevice
    {
        protected bool IsConnected;

        public abstract void ConvertAndSendReport(DS4State state, int device);

        public abstract void Connect();

        public abstract void Disconnect();

        public abstract void ResetState(bool submit = true);

        public abstract string GetDeviceType();

        public abstract void RemoveFeedbacks();

        public abstract void RemoveFeedback(int inIdx);
    }
}