namespace Vapour.Shared.Common.Types
{
    public class StickModeSettings
    {
        public FlickStickSettings FlickSettings { get; set; } = new();

        public StickControlSettings ControlSettings { get; set; } = new();
    }
}