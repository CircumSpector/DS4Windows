using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vapour.Shared.Devices.Services;
public class InputSourceBuilderService : IInputSourceBuilderService
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IServiceProvider _serviceProvider;

    public InputSourceBuilderService(
        IInputSourceConfigurationService inputSourceConfigurationService,
        IServiceProvider serviceProvider)
    {
        _inputSourceConfigurationService = inputSourceConfigurationService;
        _serviceProvider = serviceProvider;
    }

    public bool ShouldAutoCombineJoyCons { get; set; } = true;

    public List<IInputSource> BuildInputSourceList(List<ICompatibleHidDevice> controllers)
    {
        var resultList = new List<IInputSource>();

        BuildInitialInputSourceList(controllers, resultList);

        if (ShouldAutoCombineJoyCons)
        {
            AutoCombineJoyCons(controllers, resultList);
        }

        return resultList;
    }

    private void BuildInitialInputSourceList(List<ICompatibleHidDevice> controllers, List<IInputSource> resultList)
    {
        var controllersProcessed = new List<string>();

        foreach (var controller in controllers.ToList())
        {
            if (controllersProcessed.All(id => id != controller.DeviceKey))
            {
                var controllersToProcess = new List<ICompatibleHidDevice> { controller };
                var configuration =
                    _inputSourceConfigurationService.GetMultiControllerConfiguration(controller.DeviceKey);

                if (configuration != null)
                {
                    var otherControllers = controllers.Where(c =>
                        configuration.Controllers.Any(k => k.DeviceKey == c.DeviceKey) &&
                        c.DeviceKey != controller.DeviceKey).ToList();

                    if (otherControllers.Count + 1 == configuration.Controllers.Count)
                    {
                        controllersToProcess.AddRange(otherControllers);
                    }
                    else
                    {
                        configuration = null;
                    }
                }

                CreateInputSource(controllersToProcess, controllersProcessed, controllers, resultList, configuration);
            }
        }
    }

    private void AutoCombineJoyCons(List<ICompatibleHidDevice> controllers, List<IInputSource> resultList)
    {
        var leftJoyCons = GetJoyCons(typeof(JoyConLeftDeviceInfo), controllers, resultList);
        var rightJoyCons = GetJoyCons(typeof(JoyConRightDeviceInfo), controllers, resultList);

        while (leftJoyCons.Count > 0 && rightJoyCons.Count > 0)
        {
            var firstLeftJoyCon = leftJoyCons.First();
            var firstRightJoyCon = rightJoyCons.First();

            var leftDevice =
                controllers.Single(c => c.DeviceKey == firstLeftJoyCon.Configuration.Controllers[0].DeviceKey);
            var rightDevice =
                controllers.Single(c => c.DeviceKey == firstRightJoyCon.Configuration.Controllers[0].DeviceKey);

            resultList.Remove(firstLeftJoyCon);
            resultList.Remove(firstRightJoyCon);

            List<ICompatibleHidDevice> list = new()
            {
                leftDevice,
                rightDevice
            };

            CreateInputSource(list, new List<string>(), controllers, resultList);

            leftJoyCons.Remove(firstRightJoyCon);
            rightJoyCons.Remove(firstRightJoyCon);
        }
    }

    private List<IInputSource> GetJoyCons(Type deviceInfoType, List<ICompatibleHidDevice> controllers, List<IInputSource> resultList)
    {
        return (from inputSource in resultList
                where inputSource.Configuration.Controllers.Count == 1
                let controller =
                    controllers.SingleOrDefault(c => c.DeviceKey == inputSource.Configuration.Controllers[0].DeviceKey)
                where controller != null && controller.CurrentDeviceInfo.GetType() == deviceInfoType
                select inputSource).ToList();
    }


    private void CreateInputSource(List<ICompatibleHidDevice> controllersToProcess, List<string> controllersProcessed, List<ICompatibleHidDevice> controllers, List<IInputSource> resultList, InputSourceConfiguration configuration = null)
    {
        configuration ??= HackGetConfiguration(controllersToProcess);

        var inputSource = _serviceProvider.GetService<IInputSource>();
        foreach (var controller in controllersToProcess)
        {
            controller.Index = configuration.Controllers.Single(c => c.DeviceKey == controller.DeviceKey).Index;
            inputSource.AddController(controller);
            controllersProcessed.Add(controller.DeviceKey);
        }

        inputSource.LoadInputSourceConfiguration();

        resultList.Add(inputSource);
    }

    private InputSourceConfiguration HackGetConfiguration(List<ICompatibleHidDevice> controllers)
    {
        var inputSource = _serviceProvider.GetRequiredService<IInputSource>();

        foreach (var controller in controllers)
        {
            inputSource.AddController(controller);
        }

        inputSource.LoadInputSourceConfiguration();

        return inputSource.Configuration;
    }
}
