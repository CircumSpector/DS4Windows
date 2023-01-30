using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceBuilderService
{
    bool ShouldAutoCombineJoyCons { get; set; }
    List<IInputSource> BuildInputSourceList(List<ICompatibleHidDevice> controllers);
}