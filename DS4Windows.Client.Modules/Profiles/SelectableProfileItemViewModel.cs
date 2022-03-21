using System;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;

namespace DS4Windows.Client.Modules.Profiles
{
    public class SelectableProfileItemViewModel : ViewModel<ISelectableProfileItemViewModel>, ISelectableProfileItemViewModel
    {
        public string Name { get; private set; }
        public Guid Id { get; private set; }

        public void SetProfile(IProfile profile)
        {
            Name = profile.DisplayName;
            Id = profile.Id;
        }
    }
}