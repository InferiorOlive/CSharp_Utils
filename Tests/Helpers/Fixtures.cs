using System.IO;
using System.Net.Http;
using System.Text;
using NUnit.Framework;

namespace Tests.Helpers
{
    public static class Fixtures
    {
        public static string FixturesDirectory => Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures");

        public static string GetFixturePath(string fileName)
        {
            return Path.Combine(FixturesDirectory, fileName);
        }

        public static string GetFixtureText(string fixtureName)
        {
            return File.ReadAllText(GetFixturePath(fixtureName));
        }

        public static HttpContent GetHttpContent(string fixtureName)
        {
            return new StringContent(GetFixtureText(fixtureName), Encoding.UTF8, "application/json");
        }
    }
}
