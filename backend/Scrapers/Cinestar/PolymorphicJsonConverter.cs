using Newtonsoft.Json;

namespace backend.Scrapers.Cinestar
{
    public class PolymorphicJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IEnumerable<string>) || objectType == typeof(Dictionary<string, string>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(List<string>))
            {
                var list = new List<string>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray || reader.TokenType == JsonToken.EndObject)
                    {
                        return list;
                    }
                    if (reader.TokenType == JsonToken.PropertyName) continue;
                    var value = reader.Value?.ToString();
                    if (value != null)
                    {
                        list.Add(value);
                    }
                }
                return list;
            }
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
