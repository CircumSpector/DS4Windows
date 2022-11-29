using System.ComponentModel;

using AutoMapper;

using FastDeepCloner;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.Modules.Profiles.Edit;

public sealed class ProfileEditViewModel : ViewModel<IProfileEditViewModel>, IProfileEditViewModel, IDataErrorInfo
{
    private readonly IMapper _mapper;
    private readonly IProfileServiceClient _profileServiceClient;
    private readonly IViewModelFactory _viewModelFactory;

    private bool _isNew;

    private ITriggerButtonsEditViewModel _l2Button;

    private IStickEditViewModel _leftStick;

    private string _name;
    private IProfile _profile;

    private ITriggerButtonsEditViewModel _r2Button;

    private IStickEditViewModel _rightStick;

    private ISixAxisEditViewModel _sixAxisX;

    private ISixAxisEditViewModel _sixAxisZ;

    public ProfileEditViewModel(
        IViewModelFactory viewModelFactory, 
        IMapper mapper,
        IProfileServiceClient profileServiceClient)
    {
        _viewModelFactory = viewModelFactory;
        _mapper = mapper;
        _profileServiceClient = profileServiceClient;
    }

    public ITriggerButtonsEditViewModel L2Button
    {
        get => _l2Button;
        private set => SetProperty(ref _l2Button, value);
    }

    public ITriggerButtonsEditViewModel R2Button
    {
        get => _r2Button;
        private set => SetProperty(ref _r2Button, value);
    }

    public ISixAxisEditViewModel SixAxisX
    {
        get => _sixAxisX;
        private set => SetProperty(ref _sixAxisX, value);
    }

    public ISixAxisEditViewModel SixAxisZ
    {
        get => _sixAxisZ;
        private set => SetProperty(ref _sixAxisZ, value);
    }

    public override async Task Initialize()
    {
        _leftStick = await _viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
        _rightStick = await _viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
        _l2Button = await _viewModelFactory.Create<ITriggerButtonsEditViewModel, ITriggerButtonsEditView>();
        _r2Button = await _viewModelFactory.Create<ITriggerButtonsEditViewModel, ITriggerButtonsEditView>();
        _sixAxisX = await _viewModelFactory.Create<ISixAxisEditViewModel, ISixAxisEditView>();
        _sixAxisZ = await _viewModelFactory.Create<ISixAxisEditViewModel, ISixAxisEditView>();
    }

    public bool IsNew
    {
        get => _isNew;
        private set => SetProperty(ref _isNew, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public IStickEditViewModel LeftStick
    {
        get => _leftStick;
        private set => SetProperty(ref _leftStick, value);
    }

    public IStickEditViewModel RightStick
    {
        get => _rightStick;
        private set => SetProperty(ref _rightStick, value);
    }

    public string Error { get; private set; }

    public string this[string columnName]
    {
        get
        {
            string result = null;
            if (columnName == nameof(Name))
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    result = "Name is required";
                }
                else if (_profileServiceClient.ProfileList.Any(i => i.Id != _profile.Id && i.DisplayName.ToUpper() == Name.ToUpper()))
                {
                    result = "Name must be unique";
                }
            }
            Error = result;

            OnPropertyChanged(nameof(NoErrors));

            return result;
        }
    }

    public bool NoErrors
    {
        get
        {
            return string.IsNullOrEmpty(Error);
        }
    }

    public void SetProfile(IProfile profile, bool isNew = false)
    {
        _profile = profile.Clone();
        IsNew = isNew;
        _mapper.Map(_profile, this);
    }

    public IProfile GetUpdatedProfile()
    {
        _mapper.Map(this, _profile);
        return _profile;
    }
}