using System.Collections.ObjectModel;

using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.ServiceClients;

public interface IProfileServiceClient
{
    Task Initialize();
    ObservableCollection<IProfile> ProfileList { get; }
    Task<IProfile> CreateNewProfile();
    Task DeleteProfile(Guid id);
    Task<IProfile> SaveProfile(IProfile profile);
}