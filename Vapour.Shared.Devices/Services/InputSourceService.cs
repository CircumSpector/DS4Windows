using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly IInputReportProcessorService _inputReportProcessorService;
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IFilterService _filterService;

    public InputSourceService(IInputReportProcessorService inputReportProcessorService,
        IInputSourceConfigurationService inputSourceConfigurationService,
        IInputSourceDataSource inputSourceDataSource,
        IFilterService filterService)
    {
        _inputReportProcessorService = inputReportProcessorService;
        _inputSourceConfigurationService = inputSourceConfigurationService;
        _inputSourceDataSource = inputSourceDataSource;
        _filterService = filterService;
    }
    
    public void Stop()
    {
        foreach (var inputSource in _inputSourceDataSource.InputSources)
        {
            _inputReportProcessorService.StopProcessing(inputSource);
        }
    }

    public void Clear()
    {
        foreach (var inputSource in _inputSourceDataSource.InputSources.ToList())
        {
            RemoveController(inputSource.Controller1.SourceDevice.InstanceId);
        }
    }

    public void AddController(ICompatibleHidDevice device)
    {
        if (!_inputSourceDataSource.InputSources.Any(i => i.Controller1 != null && i.Controller1.DeviceKey == device.DeviceKey))
        {
            var inputSource = new InputSource { Controller1 = device };
            _inputSourceConfigurationService.LoadInputSourceConfiguration(inputSource);

            if (!_filterService.FilterUnfilterIfNeeded(device))
            {

                _inputSourceDataSource.InputSources.Add(inputSource);

                _inputReportProcessorService.StartProcessing(inputSource);

                _inputSourceDataSource.FireCreated(inputSource);
            }
        }
    }

    public void RemoveController(string instanceId)
    {
        var existing = _inputSourceDataSource.InputSources.SingleOrDefault(i => i.Controller1.SourceDevice.InstanceId.ToLower() == instanceId.ToLower());
        if (existing != null)
        {
            _inputReportProcessorService.StopProcessing(existing);
            _inputSourceDataSource.InputSources.Remove(existing);
            _inputSourceDataSource.FireRemoved(existing);
            existing.Controller1.Dispose();
        }
    }
}