using System.Threading;
using System.Threading.Tasks;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Devices.HostedServices;

/// <summary>
///     Manages compatible input device detection and state handling.
/// </summary>
public class ControllerManagerHost : IHostedService
{
    private readonly IControllersEnumeratorService enumerator;

    private readonly IInputSourceService inputSourceService;

    private readonly ILogger<ControllerManagerHost> logger;

    private readonly IControllerManagerService manager;

    private readonly IProfilesService profileService;
    
    public ControllerManagerHost(IControllersEnumeratorService enumerator,
        ILogger<ControllerManagerHost> logger, IProfilesService profileService,
        IControllerManagerService manager, IInputSourceService inputSourceService)
    {
        this.enumerator = enumerator;
        this.logger = logger;
        this.profileService = profileService;
        this.manager = manager;
        this.inputSourceService = inputSourceService;

        enumerator.ControllerReady += EnumeratorServiceOnControllerReady;
        enumerator.ControllerRemoved += EnumeratorOnControllerRemoved;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //
        // Make sure we're ready to rock
        // 
        profileService.Initialize();

        logger.LogInformation("Starting device enumeration");

        //
        // Run full enumeration only once at startup, during runtime arrival events are used
        // 
        await Task.Run(() => enumerator.EnumerateDevices(), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets invoked when a new compatible device is detected.
    /// </summary>
    private void EnumeratorServiceOnControllerReady(ICompatibleHidDevice device)
    {
        var slotIndex = manager.AssignFreeSlotWith(device);

        if (slotIndex == -1)
        {
            logger.LogError("No free slot available");
            return;
        }

        profileService.ControllerArrived(slotIndex, device.Serial);
    }

    /// <summary>
    ///     Gets invoked when a compatible device has departed.
    /// </summary>
    private void EnumeratorOnControllerRemoved(ICompatibleHidDevice device)
    {
        var slot = manager.FreeSlotContaining(device);

        profileService.ControllerDeparted(slot, device.Serial);
    }
}