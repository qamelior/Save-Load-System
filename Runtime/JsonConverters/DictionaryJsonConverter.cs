using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace SaveLoadSystem.JsonConverters
{
    public class DictionaryJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not IDictionary dictionary) return;
            writer.WriteStartArray();

            foreach (var key in dictionary.Keys)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Key");
                serializer.Serialize(writer, key);
                writer.WritePropertyName("Value");
                serializer.Serialize(writer, dictionary[key]);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!CanConvert(objectType))
                throw new Exception($"This converter is not for {objectType}.");

            var keyType = objectType.GetGenericArguments()[0];
            var valueType = objectType.GetGenericArguments()[1];
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var result = (IDictionary)Activator.CreateInstance(dictionaryType);

            if (reader.TokenType == JsonToken.Null)
                return null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.EndArray:
                        return result;
                    case JsonToken.StartObject:
                        AddObjectToDictionary(reader, result, serializer, keyType, valueType);
                        break;
                }
            }

            return result;
        }

        public override bool CanConvert(Type objectType) => objectType.IsGenericType && (objectType.GetGenericTypeDefinition() == typeof(IDictionary<,>) || objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>));

        private void AddObjectToDictionary(JsonReader reader, IDictionary result, JsonSerializer serializer, Type keyType, Type valueType)
        {
            object key = null;
            object value = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject && key != null)
                {
                    result.Add(key, value);
                    return;
                }
                
                switch (reader.Value?.ToString())
                {
                    case "Key":
                        reader.Read();
                        key = serializer.Deserialize(reader, keyType);
                        break;
                    case "Value":
                        reader.Read();
                        value = serializer.Deserialize(reader, valueType);
                        break;
                }
            }
        }
    }
}