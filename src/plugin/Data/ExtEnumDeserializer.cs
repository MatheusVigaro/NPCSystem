using System;
using Newtonsoft.Json;

namespace NPCSystem;

public class ExtEnumDeserializer<T> : JsonConverter where T : ExtEnum<T>
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.String:
                return (T)Activator.CreateInstance(typeof(T), (string)serializer.Deserialize(reader, typeof(string)), true);
            default:
                return null;
        }
   }

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => false;
}