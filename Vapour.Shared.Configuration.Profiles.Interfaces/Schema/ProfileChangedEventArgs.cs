namespace Vapour.Shared.Configuration.Profiles.Schema;
public class ProfileChangedEventArgs : EventArgs
{
    public ProfileChangedEventArgs(string controllerKey, IProfile oldProfile, IProfile newProfile)
    {
        ControllerKey = controllerKey;
        OldProfile = oldProfile;
        NewProfile = newProfile;    
    }

    public string ControllerKey { get; }
    public IProfile OldProfile { get; }
    public IProfile NewProfile { get; }
}
