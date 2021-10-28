using System;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    /// <summary>
    ///     Represents application-wide settings.
    /// </summary>
    public partial class DS4WindowsAppSettingsV3 : XmlSerializable<DS4WindowsAppSettingsV3>
    {
        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .EnableReferences()
                .WithUnknownContent().Continue()
                .EnableImplicitTyping(typeof(DS4WindowsAppSettingsV3))
                .Type<DS4Color>().Register().Converter().Using(DS4ColorConverter.Default)
                .Type<bool>().Register().Converter().Using(BooleanConverter.Default)
                .Type<CustomLedProxyType>().Register().Converter().Using(CustomLedConverter.Default)
                .Type<DateTime>().Register().Converter().Using(DateTimeConverter.Default)
                .Type<Version>().Register().Converter().Using(VersionConverter.Default)
                .Create();
        }

        /// <summary>
        ///     Actions necessary prior to serialization (like type conversions).
        /// </summary>
        public DS4WindowsAppSettingsV3 PreSerialization()
        {
            CustomLed1 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[0].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[0].Ds4WinSettings.Led
            };
            CustomLed2 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[1].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[1].Ds4WinSettings.Led
            };
            CustomLed3 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[2].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[2].Ds4WinSettings.Led
            };
            CustomLed4 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[3].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[3].Ds4WinSettings.Led
            };
            CustomLed5 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[4].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[4].Ds4WinSettings.Led
            };
            CustomLed6 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[5].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[5].Ds4WinSettings.Led
            };
            CustomLed7 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[6].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[6].Ds4WinSettings.Led
            };
            CustomLed8 = new CustomLedProxyType
            {
                IsEnabled = LightbarSettingInfo[7].Ds4WinSettings.UseCustomLed,
                CustomColor = LightbarSettingInfo[7].Ds4WinSettings.Led
            };

            return this;
        }

        /// <summary>
        ///     Actions necessary after deserialization (like type conversions).
        /// </summary>
        public DS4WindowsAppSettingsV3 PostDeserialization()
        {
            LightbarSettingInfo[0].Ds4WinSettings.UseCustomLed = CustomLed1.IsEnabled;
            LightbarSettingInfo[0].Ds4WinSettings.Led = CustomLed1.CustomColor;
            LightbarSettingInfo[1].Ds4WinSettings.UseCustomLed = CustomLed2.IsEnabled;
            LightbarSettingInfo[1].Ds4WinSettings.Led = CustomLed2.CustomColor;
            LightbarSettingInfo[2].Ds4WinSettings.UseCustomLed = CustomLed3.IsEnabled;
            LightbarSettingInfo[2].Ds4WinSettings.Led = CustomLed3.CustomColor;
            LightbarSettingInfo[3].Ds4WinSettings.UseCustomLed = CustomLed4.IsEnabled;
            LightbarSettingInfo[3].Ds4WinSettings.Led = CustomLed4.CustomColor;
            LightbarSettingInfo[4].Ds4WinSettings.UseCustomLed = CustomLed5.IsEnabled;
            LightbarSettingInfo[4].Ds4WinSettings.Led = CustomLed5.CustomColor;
            LightbarSettingInfo[5].Ds4WinSettings.UseCustomLed = CustomLed6.IsEnabled;
            LightbarSettingInfo[5].Ds4WinSettings.Led = CustomLed6.CustomColor;
            LightbarSettingInfo[6].Ds4WinSettings.UseCustomLed = CustomLed7.IsEnabled;
            LightbarSettingInfo[6].Ds4WinSettings.Led = CustomLed7.CustomColor;
            LightbarSettingInfo[7].Ds4WinSettings.UseCustomLed = CustomLed8.IsEnabled;
            LightbarSettingInfo[7].Ds4WinSettings.Led = CustomLed8.CustomColor;

            return this;
        }

        /// <summary>
        ///     Converts properties from <see cref="IBackingStore" /> to this <see cref="DS4WindowsAppSettingsV3" /> instance.
        /// </summary>
        /// <param name="store">The <see cref="IBackingStore" />.</param>
        [IntermediateSolution]
        public void CopyFrom(IBackingStore store)
        {
        }

        /// <summary>
        ///     Injects properties from <see cref="DS4WindowsAppSettingsV3" /> into <see cref="IBackingStore" /> instance.
        /// </summary>
        /// <param name="store">The <see cref="IBackingStore" />.</param>
        [IntermediateSolution]
        public void CopyTo(IBackingStore store)
        {
            for (var i = 0; i < Global.MAX_DS4_CONTROLLER_COUNT; i++)
            {
                var value = (string)GetType().GetProperty($"Controller{i + 1}")?.GetValue(this, null);

                if (string.IsNullOrEmpty(value))
                {
                    store.ProfilePath[i] = store.OlderProfilePath[i] = string.Empty;
                    store.DistanceProfiles[i] = false;
                }
                else
                {
                    store.ProfilePath[i] = value;
                    if (store.ProfilePath[i].ToLower().Contains("distance"))
                        store.DistanceProfiles[i] = true;

                    store.OlderProfilePath[i] = store.ProfilePath[i];
                }
            }
        }
    }
}