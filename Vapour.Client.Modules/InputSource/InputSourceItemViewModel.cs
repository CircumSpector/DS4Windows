using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using AutoMapper;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.InputSource;

namespace Vapour.Client.Modules.InputSource;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class InputSourceItemViewModel :
    ViewModel<IInputSourceItemViewModel>,
    IInputSourceItemViewModel
{
    private readonly IInputSourceServiceClient _inputSourceServiceClient;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IMapper _mapper;

    public InputSourceItemViewModel(IMapper mapper, IInputSourceServiceClient inputSourceServiceClient, IViewModelFactory viewModelFactory)
    {
        _mapper = mapper;
        _inputSourceServiceClient = inputSourceServiceClient;
        _viewModelFactory = viewModelFactory;
    }

    public async Task SetInputSource(InputSourceMessage inputSource)
    {
        ConfigurationSetFromUser = false;
        _mapper.Map(inputSource, this);

        var controller1 = await _viewModelFactory.CreateViewModel<IInputSourceControllerItemViewModel>();
        _mapper.Map(inputSource.Controller1, controller1);
        Controller1 = controller1;

        if (inputSource.Controller2 != null)
        {
            var controller2 = await _viewModelFactory.CreateViewModel<IInputSourceControllerItemViewModel>();
            _mapper.Map(inputSource.Controller2, controller2);
            Controller2 = controller2;
        }

        ConfigurationSetFromUser = true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedProfileId) ||
            e.PropertyName == nameof(IsPassthru) ||
            e.PropertyName == nameof(OutputDeviceType))
        {
            if (ConfigurationSetFromUser)
            {
                if (!CurrentConfiguration.IsGameConfiguration)
                {
                    _inputSourceServiceClient.SaveDefaultInputSourceConfiguration(InputSourceKey, CurrentConfiguration);
                }
                else
                {
                    _inputSourceServiceClient.SaveGameConfiguration(InputSourceKey, CurrentConfiguration.GameInfo,
                        CurrentConfiguration);
                }
            }
        }
        else if (e.PropertyName == nameof(Controller2))
        {
            OnPropertyChanged(nameof(HasSecondController));
        }

        base.OnPropertyChanged(e);
    }
}