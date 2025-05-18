using System;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveLoadSystem.JsonConverters
{
    public class Vector2IntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Vector2Int);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var temp = JObject.Load(reader);
            return new Vector2Int(((int?)temp["X"]).GetValueOrDefault(), ((int?)temp["Y"]).GetValueOrDefault());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not Vector2Int vector) return;
            serializer.Serialize(writer, new { X = vector.x, Y = vector.y });
        }
    }
}