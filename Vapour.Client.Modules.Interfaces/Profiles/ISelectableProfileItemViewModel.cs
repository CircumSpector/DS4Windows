using System;
using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.Modules.Profiles;

public interface ISelectableProfileItemViewModel : IViewModel<ISelectableProfileItemViewModel>
{
    string Name { get; }

    Guid Id { get; }

    void SetProfile(IProfile profile);
}