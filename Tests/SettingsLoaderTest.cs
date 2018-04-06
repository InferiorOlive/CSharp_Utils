using System;
using System.Collections.Generic;
using LabUtilities;
using NUnit.Framework;
using Tests.Helpers;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Tests.LabUtilities
{
    [TestFixture]
    public class SettingsLoaderTest
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestSettings
        {
            public string SomeApiKey { get; set; }
            public string SomethingElse { get; set; }
            public int ThisIsAnInt { get; set; }
#pragma warning disable 649
            public string FieldShouldBeSet;
#pragma warning restore 649
        }

        private IDictionary<string, string> _oldEnvironment;

        [SetUp]
        public void SetUp()
        {
            _oldEnvironment = new Dictionary<string, string>();
            var env = Environment.GetEnvironmentVariables();
            foreach (var key in env.Keys)
            {
                _oldEnvironment[key.ToString()] = env[key].ToString();
                Environment.SetEnvironmentVariable(key.ToString(), null);
            }
        }

        [TearDown]
        public void TearDown()
        {
            var env = Environment.GetEnvironmentVariables();
            foreach (var key in env.Keys)
            {
                Environment.SetEnvironmentVariable(key.ToString(), null);
            }

            foreach (var kvp in _oldEnvironment)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }
        }

        [Test]
        public void TestLoadFullJson()
        {
            Console.WriteLine(Fixtures.GetFixturePath("FullSettingsJson.json"));

            var settings = new SettingsLoader<TestSettings>()
                .UseJson(Fixtures.GetFixturePath("FullSettingsJson.json"))
                .LoadSettings();

            Assert.That(settings.SomeApiKey, Is.EqualTo("this-is-an-api-key"));
            Assert.That(settings.SomethingElse, Is.EqualTo("something-else"));
            Assert.That(settings.ThisIsAnInt, Is.EqualTo(230));
            Assert.That(settings.FieldShouldBeSet, Is.EqualTo("sure are!"));
        }

        [Test]
        public void TestLoadFromEnv()
        {
            Environment.SetEnvironmentVariable("FOO_SOME_API_KEY", "api-key-from-env");
            Environment.SetEnvironmentVariable("FOO_THIS_IS_AN_INT", "512");
            Environment.SetEnvironmentVariable("FOO_FIELD_SHOULD_BE_SET", "they are");

            var settings = new SettingsLoader<TestSettings>()
                .UseEnvironmentVariables("FOO_")
                .LoadSettings();
            
            Assert.That(settings.SomeApiKey, Is.EqualTo("api-key-from-env"));
            Assert.That(settings.ThisIsAnInt, Is.EqualTo(512));
            Assert.That(settings.SomethingElse, Is.Null);
            Assert.That(settings.FieldShouldBeSet, Is.EqualTo("they are"));
        }

        [Test]
        public void TestLoadFromBoth()
        {
            Environment.SetEnvironmentVariable("FOO_SOMETHING_ELSE", "from env");
            var settings = new SettingsLoader<TestSettings>()
                .UseEnvironmentVariables("FOO_")
                .UseJson(Fixtures.GetFixturePath("FullSettingsJson.json"))
                .LoadSettings();

            Assert.That(settings.SomeApiKey, Is.EqualTo("this-is-an-api-key"));
            Assert.That(settings.SomethingElse, Is.EqualTo("from env"), "ENV takes precedence");
        }
    }
}
