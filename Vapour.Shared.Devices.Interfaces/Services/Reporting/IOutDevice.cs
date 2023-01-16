using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

public interface IOutDevice
{
    void ConvertAndSendReport(InputSourceReport state, int device = 0);
    void Connect();
    void Disconnect();
    void ResetState(bool submit = true);
    OutputDeviceType GetDeviceType();
    void RemoveFeedbacks();
    void RemoveFeedback(int inIdx);
}