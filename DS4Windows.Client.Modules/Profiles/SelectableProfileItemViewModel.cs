using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using System;

namespace DS4Windows.Client.Modules.Profiles
{
    public class SelectableProfileItemViewModel : ViewModel<ISelectableProfileItemViewModel>, ISelectableProfileItemViewModel
    {
        private DS4WindowsProfile? profile;

        public string? Name { get; private set; }

        public Guid? Id { get; private set; }

        public void SetProfile(DS4WindowsProfile? profile)
        {
            this.profile = profile;

            Name = profile.DisplayName;
            Id = profile.Id;
        }
    }
}
