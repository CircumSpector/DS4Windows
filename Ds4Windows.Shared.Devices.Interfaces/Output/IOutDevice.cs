using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace Ds4Windows.Shared.Devices.Interfaces.Output;

public interface IOutDevice
{
    void ConvertAndSendReport(CompatibleHidDeviceInputReport state, int device = 0);
    void Connect();
    void Disconnect();
    void ResetState(bool submit = true);
    string GetDeviceType();
    void RemoveFeedbacks();
    void RemoveFeedback(int inIdx);
}