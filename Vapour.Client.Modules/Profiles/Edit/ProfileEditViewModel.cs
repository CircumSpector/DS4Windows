using System.ComponentModel;

using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;

using FastDeepCloner;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.Modules.Profiles.Edit;

public sealed partial class ProfileEditViewModel : ViewModel<IProfileEditViewModel>, IProfileEditViewModel, IDataErrorInfo
{
    private readonly IMapper _mapper;
    private readonly IProfileServiceClient _profileServiceClient;
    private readonly IViewModelFactory _viewModelFactory;

    private IProfile _profile;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private ITriggerButtonsEditViewModel _l2Button;

    [ObservableProperty]
    private IStickEditViewModel _leftStick;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private ITriggerButtonsEditViewModel _r2Button;

    [ObservableProperty]
    private IStickEditViewModel _rightStick;

    [ObservableProperty]
    private ISixAxisEditViewModel _sixAxisX;

    [ObservableProperty]
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

    public override async Task Initialize()
    {
        _leftStick = await _viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
        _rightStick = await _viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
        _l2Button = await _viewModelFactory.Create<ITriggerButtonsEditViewModel, ITriggerButtonsEditView>();
        _r2Button = await _viewModelFactory.Create<ITriggerButtonsEditViewModel, ITriggerButtonsEditView>();
        _sixAxisX = await _viewModelFactory.Create<ISixAxisEditViewModel, ISixAxisEditView>();
        _sixAxisZ = await _viewModelFactory.Create<ISixAxisEditViewModel, ISixAxisEditView>();
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