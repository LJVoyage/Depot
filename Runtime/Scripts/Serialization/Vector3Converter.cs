
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ForgeCLR.Plugins.Depot.Runtime.Serialization
{
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            JObject obj = new JObject
            {
                { "x", value.x },
                { "y", value.y },
                { "z", value.z }
            };
            obj.WriteTo(writer);
        }

        public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new Vector3(
                obj["x"]?.Value<float>() ?? 0,
                obj["y"]?.Value<float>() ?? 0,
                obj["z"]?.Value<float>() ?? 0
            );
        }
    }
}