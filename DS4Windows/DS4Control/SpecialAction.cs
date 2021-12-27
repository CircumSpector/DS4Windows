using System;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using Newtonsoft.Json;
using PropertyChanged;

namespace DS4WinWPF.DS4Control
{
    [JsonConverter(typeof(SpecialActionsConverter))]
    [AddINotifyPropertyChangedInterface]
    public abstract class SpecialAction : IEquatable<SpecialAction>
    {
        public static SpecialAction Key = new SpecialActionKey();
        public static SpecialAction Program = new SpecialActionProgram();
        public static SpecialAction Profile = new SpecialActionProfile();
        public static SpecialAction Macro = new SpecialActionMacro();
        public static SpecialAction DisconnectBluetooth = new SpecialActionDisconnectBluetooth();
        public static SpecialAction BatteryCheck = new SpecialActionBatteryCheck();
        public static SpecialAction MultiAction = new SpecialActionMultiAction();
        public static SpecialAction XboxGameDVR = new SpecialActionXboxGameDVR();
        public static SpecialAction SteeringWheelEmulationCalibrate = new SpecialActionSteeringWheelEmulationCalibrate();

        /// <summary>
        ///     The <see cref="SpecialAction"/> type to help with (de-)serialization.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        ///     A unique ID to guarantee uniqueness of this <see cref="SpecialAction"/>.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        ///     User-defined display name of this <see cref="SpecialAction"/>.
        /// </summary>
        public string DisplayName { get; set; }

        protected SpecialAction() { }

        public bool Equals(SpecialAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SpecialAction other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class SpecialActionKey : SpecialAction
    {
        public override string Type => nameof(SpecialActionKey);
    }

    public class SpecialActionProgram : SpecialAction
    {
        public override string Type => nameof(SpecialActionProgram);
    }

    public class SpecialActionProfile : SpecialAction
    {
        public override string Type => nameof(SpecialActionProfile);
    }

    public class SpecialActionMacro : SpecialAction
    {
        public override string Type => nameof(SpecialActionMacro);
    }

    public class SpecialActionDisconnectBluetooth : SpecialAction
    {
        public override string Type => nameof(SpecialActionDisconnectBluetooth);
    }

    public class SpecialActionBatteryCheck : SpecialAction
    {
        public override string Type => nameof(SpecialActionBatteryCheck);
    }

    public class SpecialActionMultiAction : SpecialAction
    {
        public override string Type => nameof(SpecialActionMultiAction);
    }

    public class SpecialActionXboxGameDVR : SpecialAction
    {
        public override string Type => nameof(SpecialActionXboxGameDVR);
    }

    public class SpecialActionSteeringWheelEmulationCalibrate : SpecialAction
    {
        public override string Type => nameof(SpecialActionSteeringWheelEmulationCalibrate);
    }
}
