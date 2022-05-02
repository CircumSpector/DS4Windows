namespace DS4Windows.Shared.Devices.DriverManagement;

public interface IControllerDriverManagementService
{
    void HideController(string controllerInstanceId);
    void UnhideController(string controllerInstanceId);
}