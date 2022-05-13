using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DS4Windows.Shared.Configuration.Profiles.Schema;

namespace DS4Windows.Client.Modules.Profiles.Utils;

public interface IProfileServiceClient
{
    Task Initialize();
    ObservableCollection<IProfile> ProfileList { get; }
    Task<IProfile> CreateNewProfile();
    Task DeleteProfile(Guid id);
    Task<IProfile> SaveProfile(IProfile profile);
}