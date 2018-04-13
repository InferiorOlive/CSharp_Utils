using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using NUnit.Framework;
using LabUtilities;
using Tests.Helpers;

namespace Tests.LabUtilities
{
    [TestFixture]
    public class JsonPathConverterTest
    {
        [JsonConverter(typeof(JsonPathConverter))]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class SingleArrayTest
        {
            public List<string> Strings { get; set; }
        }


        private const string SupriseItsAnObject = @"
           { ""strings"": ""haha i'm an array"" }
        ";

        private const string ItsActuallyAList = @"
            { ""strings"": [""why""] }
        ";

        [JsonConverter(typeof(JsonPathConverter))]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class ObjectToDeserialize
        {
            [JsonProperty("foo.bar")] public int FooBar { get; set; }
            [JsonProperty("really.deeply.nested")] public int ReallyDeeplyNested { get; set; }

            public int ThisIsntInTheJSON { get; set; }
            public string ThisIsntInTheJSONEither { get; set; }
            public int CamelCaseConversion { get; set; }
#pragma warning disable 649 // Unused, no default value
            public int FieldsShouldWork;
#pragma warning restore 649
        }

        private const string NestedPropertyJson = @"{
            ""foo"": {
                ""bar"": 33
            },
            ""really"":{""deeply"":{""nested"":42}},
            ""camelCaseConversion"": 101,
            ""fieldsShouldWork"": 501
            }
        ";


        [Test]
        public void TestDeserializeNested()
        {
            var result = Json.Deserialize<ObjectToDeserialize>(NestedPropertyJson);
            Assert.That(result.FooBar, Is.EqualTo(33));
            Assert.That(result.ReallyDeeplyNested, Is.EqualTo(42));
            Assert.That(result.CamelCaseConversion, Is.EqualTo(101));
            Assert.That(result.FieldsShouldWork, Is.EqualTo(501));
            Assert.That(result.ThisIsntInTheJSON, Is.EqualTo(0));
            Assert.IsNull(result.ThisIsntInTheJSONEither);
        }

        [Test]
        public void TestDeserializeSingleObject()
        {
            var result = Json.Deserialize<SingleArrayTest>(SupriseItsAnObject);
            Assert.That(result.Strings.Count, Is.EqualTo(1));
            Assert.That(result.Strings[0], Is.EqualTo("haha i'm an array"));

            result = Json.Deserialize<SingleArrayTest>(ItsActuallyAList);
            Assert.That(result.Strings.Count, Is.EqualTo(1));
            Assert.That(result.Strings[0], Is.EqualTo("why"));
        }
    }
}
