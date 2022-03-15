using System;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;

namespace DS4Windows.Client.Modules.Profiles;

public class SelectableProfileItemViewModel : ViewModel<ISelectableProfileItemViewModel>,
    ISelectableProfileItemViewModel
{
    public class SelectableProfileItemViewModel : ViewModel<ISelectableProfileItemViewModel>, ISelectableProfileItemViewModel
    {
        private IProfile? profile;

    public string? Name { get; private set; }

    public Guid? Id { get; private set; }

        public void SetProfile(IProfile? profile)
        {
            this.profile = profile;

        Name = profile.DisplayName;
        Id = profile.Id;
    }
}