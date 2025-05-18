using System;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveLoadSystem.JsonConverters
{
    public class QuaternionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Quaternion);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var temp = JObject.Load(reader);
            return new Quaternion(((float?)temp["X"]).GetValueOrDefault(), ((float?)temp["Y"]).GetValueOrDefault(),
                ((float?)temp["Z"]).GetValueOrDefault(), ((float?)temp["W"]).GetValueOrDefault());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not Quaternion quaternion) return;
            serializer.Serialize(writer, new { X = quaternion.x, Y = quaternion.y, Z = quaternion.z, W = quaternion.w });
        }
    }
}