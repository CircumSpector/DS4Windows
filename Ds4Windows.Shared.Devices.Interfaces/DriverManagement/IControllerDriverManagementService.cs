namespace Ds4Windows.Shared.Devices.Interfaces.DriverManagement;

public interface IControllerDriverManagementService
{
    void HideController(string controllerInstanceId);
    void UnhideController(string controllerInstanceId);
}