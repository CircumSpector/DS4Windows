using System.Windows.Media;

using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Configuration.Profiles;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Types;

namespace Vapour.Client.Modules.Profiles;

public sealed partial class ProfileListItemViewModel :
    ViewModel<IProfileListItemViewModel>,
    IProfileListItemViewModel
{
    private readonly IMapper _mapper;
    private IProfile _profile;

    public ProfileListItemViewModel(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void SetProfile(IProfile profile)
    {
        _profile = profile;
        _profile.ProfilePropertyChanged += Profile_ProfilePropertyChanged;

        _mapper.Map(profile, this);
    }

    private void Profile_ProfilePropertyChanged(object sender, ProfilePropertyChangedEventArgs e)
    {
        _mapper.Map(_profile, this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _profile.ProfilePropertyChanged -= Profile_ProfilePropertyChanged;
            _profile = null;
        }

        base.Dispose(disposing);
    }

    #region Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotIsDefaultProfile))]
    private Guid _id;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _outputControllerType;

    [ObservableProperty]
    private SolidColorBrush _lightbarColor;

    [ObservableProperty]
    private string _touchpadMode;

    [ObservableProperty]
    private string _gyroMode;

    public bool NotIsDefaultProfile => Id != Constants.DefaultProfileId;

    #endregion
}