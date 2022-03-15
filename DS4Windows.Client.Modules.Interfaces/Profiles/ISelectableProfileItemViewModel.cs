using System;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;

namespace DS4Windows.Client.Modules.Profiles;

public interface ISelectableProfileItemViewModel : IViewModel<ISelectableProfileItemViewModel>
{
    string? Name { get; }

    Guid? Id { get; }

    void SetProfile(IProfile? profile);
}