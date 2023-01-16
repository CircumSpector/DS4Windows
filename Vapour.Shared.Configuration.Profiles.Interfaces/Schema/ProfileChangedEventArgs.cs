namespace Vapour.Shared.Configuration.Profiles.Schema;
public class ProfileChangedEventArgs : EventArgs
{
    public ProfileChangedEventArgs(string inputSourceKey, IProfile oldProfile, IProfile newProfile)
    {
        InputSourceKey = inputSourceKey;
        OldProfile = oldProfile;
        NewProfile = newProfile;    
    }

    public string InputSourceKey { get; }
    public IProfile OldProfile { get; }
    public IProfile NewProfile { get; }
}
