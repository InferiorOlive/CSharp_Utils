using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LabUtilities
{
    /// <summary>
    /// Lets us use paths to properties in JsonProperty attributes.
    /// </summary>
    /// <example>
    /// <code>
    ///  [JsonConverter(typeof(JsonPathConverter))]
    ///  class Person {
    ///     [JsonProperty("address.country")]
    ///     public string Country {get; set;}
    ///  }
    ///  
    ///  // ...
    ///   
    ///  var person = JsonConvert.DeserializeObject&lt;Person&gt;(
    ///     @"{ 
    ///         ""address"": {
    ///             ""country"": ""Norway""
    ///         }
    ///       ");
    ///   person.Country // => "Norway"
    /// 
    /// </code>
    /// </example>
    public class JsonPathConverter : JsonConverter
    {
        // We don't write objects - it's possible, but adds a bunch of complexity
        public override bool CanWrite => false;

        // This is never called because CanWrite is false.
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObj = JObject.Load(reader);
            var result = Activator.CreateInstance(objectType);

            foreach (var prop in GetInterestingMembers(objectType))
            {
                // If it has a JsonPropertyAttribute we'll use that name
                var attribute = prop.GetCustomAttributes(true)
                    .OfType<JsonPropertyAttribute>()
                    .FirstOrDefault();
                var jsonPath = attribute != null ? attribute.PropertyName : prop.Name;

                // Let the Contractresolver handle the name
                if (serializer.ContractResolver is DefaultContractResolver resolver)
                {
                    jsonPath = resolver.GetResolvedPropertyName(jsonPath);
                }

                // Leave 

                if (!Regex.IsMatch(jsonPath, @"^[a-zA-Z0-9_.-@]+$"))
                {
                    throw new InvalidOperationException("JsonProperties of JsonPathConverter can have only " +
                                                        "have alphanumeric, dots, and dashes but name was'" + jsonPath + "'.");
                }

                var token = jObj.SelectToken(jsonPath);
                if (token != null && token.Type != JTokenType.Null)
                {
                    SetValue(result, prop, token);
                }
            }

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            // You must use this class via the [JsonConverter] attribute.
            return false;
        }

        private static void SetValue(object target, MemberInfo member, JToken value)
        {
            Type type;
            switch (member)
            {
                case PropertyInfo prop:
                    type = prop.PropertyType;
                    prop.SetValue(target, ConvertSingleToArray(value, type));
                    break;
                case FieldInfo field:
                    type = field.FieldType;
                    field.SetValue(target, ConvertSingleToArray(value, type));
                    break;
                default:
                    throw new InvalidOperationException($"Invalid member type: {member} (expected PropertyInfo or FieldInfo)");
            }
        }

        private static object ConvertSingleToArray(JToken value, Type type)
        {
            // Sometimes the live person API returns a single object; but we need a
            // *list* of such elements!

            // We want a list; but we don't have one.  Wrap it up!
            if (typeof(IList).IsAssignableFrom(type) && !(value is JArray))
            {
                var element = value.ToObject(type.GetGenericArguments()[0]);
                var list = (IList)Activator.CreateInstance(type);
                list.Add(element);
                return list;
            }

            return value.ToObject(type);
        }

        private static IEnumerable<MemberInfo> GetInterestingMembers(Type type)
        {
            return type.GetMembers().Where(m =>
                m is PropertyInfo prop && prop.CanRead && prop.CanWrite ||
                m is FieldInfo field && field.IsPublic
            );
        }
    }
}
