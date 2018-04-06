using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Tests.Helpers
{
    /// <summary>
    /// Some helpers for getting the names right in our JSON
    /// </summary>
    public static class Json
    {
        private static readonly Lazy<JsonSerializerSettings> LazyDefaultSerializerSettings = 
            new Lazy<JsonSerializerSettings>(() =>
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            settings.Converters.Add(new StringEnumConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            return settings;
        });

        public static JsonSerializerSettings DefaultSerializerSettings => LazyDefaultSerializerSettings.Value;

        public static JsonSerializer DefaultSerializer => JsonSerializer.Create(DefaultSerializerSettings);
        

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, DefaultSerializerSettings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, DefaultSerializerSettings);
        }
    }
}
