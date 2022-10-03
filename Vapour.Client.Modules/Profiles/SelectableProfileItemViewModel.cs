using System;
using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.Modules.Profiles
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