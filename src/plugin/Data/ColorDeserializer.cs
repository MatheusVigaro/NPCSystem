using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NPCSystem;

public class ColorDeserializer : JsonConverter
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
                if (ColorUtility.TryParseHtmlString((string)serializer.Deserialize(reader, typeof(string)), out var color))
                {
                    return color;
                }
                return null;
            default:
                return null;
        }
   }

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => false;
}