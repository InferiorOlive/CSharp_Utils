using System;
using NUnit.Framework;
using LabUtilities;
using Tests.Helpers;

namespace Tests.LabUtilities
{
    [TestFixture]
    public class JsonTests
    {
        [Test]
        public void TestJson()
        {
            var json = new JsonPathConverter();
            Assert.That(Json.Serialize(json), Is.EqualTo("{\"canWrite\":false,\"canRead\":true}"));

            DateTime testDate = new DateTime(2018, 1, 2);
            var deserialized = Json.Deserialize<DateTime>("\"2018-01-02T00:00:00\"");
            Assert.That(deserialized, Is.EqualTo(testDate));
        }
    }
}
