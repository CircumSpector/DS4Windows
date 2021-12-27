using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    public class SpecialActionClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            return typeof(SpecialAction).IsAssignableFrom(objectType) && !objectType.IsAbstract
                ? null
                : base.ResolveContractConverter(objectType);
        }
    }

    internal class SpecialActionsConverter : JsonConverter<SpecialAction>
    {
        private static readonly JsonSerializerSettings SpecialActionConversion =
            new()
            {
                ContractResolver = new SpecialActionClassConverter()
            };

        public override bool CanWrite { get; } = false;

        public override void WriteJson(JsonWriter writer, SpecialAction? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override SpecialAction? ReadJson(JsonReader reader, Type objectType, SpecialAction? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            return jo["Type"].Value<string>() switch
            {
                nameof(SpecialActionKey) => JsonConvert.DeserializeObject<SpecialActionKey>(jo.ToString(),
                    SpecialActionConversion),
                nameof(SpecialActionProgram) => JsonConvert.DeserializeObject<SpecialActionProgram>(jo.ToString(),
                    SpecialActionConversion),
                nameof(SpecialActionProfile) => JsonConvert.DeserializeObject<SpecialActionProfile>(jo.ToString(),
                    SpecialActionConversion),
                nameof(SpecialActionMacro) => JsonConvert.DeserializeObject<SpecialActionMacro>(jo.ToString(),
                    SpecialActionConversion),
                nameof(SpecialActionDisconnectBluetooth) => JsonConvert
                    .DeserializeObject<SpecialActionDisconnectBluetooth>(jo.ToString(), SpecialActionConversion),
                nameof(SpecialActionBatteryCheck) => JsonConvert.DeserializeObject<SpecialActionBatteryCheck>(
                    jo.ToString(), SpecialActionConversion),
                nameof(SpecialActionMultiAction) => JsonConvert.DeserializeObject<SpecialActionMultiAction>(
                    jo.ToString(), SpecialActionConversion),
                nameof(SpecialActionXboxGameDVR) => JsonConvert.DeserializeObject<SpecialActionXboxGameDVR>(
                    jo.ToString(), SpecialActionConversion),
                nameof(SpecialActionSteeringWheelEmulationCalibrate) => JsonConvert
                    .DeserializeObject<SpecialActionSteeringWheelEmulationCalibrate>(jo.ToString(),
                        SpecialActionConversion),
                _ => null
            };
        }
    }
}