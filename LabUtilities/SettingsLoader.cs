using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LivePersonNet.Utilities
{
    /// <summary>
    /// Provides a generic way to load settings from JSON files and environment variables.
    /// </summary>
    /// <remarks>
    ///     The JSON and environment settings are used according to the following semantics:
    ///         - Values present in the environment will overwrite those found in the settings file.
    ///         - If the JSON file does not exist, and message will be logged at the <see cref="LogLevel.Trace"/>
    ///           severity, and the resulting object's properties are left as their default values.
    ///         - Environment variables that start with the given prefix are converted from 
    ///           <code>SCREAMING_SNAKE_CASE</code> to <code>UpperCamelCase</code> after stripping the prefix
    ///           to map them to corresponding fields.
    /// </remarks>
    /// <example>
    /// 
    /// <code>
    /// public class MySettings 
    /// {
    ///     public string SomeApiKey {get; set;}
    ///     public string SomeOtherApiKey { get; set; }
    ///     public int MaxRetryCount { get; set; }
    ///     public string FieldsWorkToo;
    /// }
    /// 
    /// // Assume that settings.json contains {"SomeApiKey": "api-key-from-json", "SomeOtherApiKey": "from json"}, and that the 
    /// // environment variables FOO_SOME_API_KEY=api-key-from-env,
    /// // FOO_MAX_RETRY_COUNT=10, and FOO_FIELDS_WORK_TOO="Sure do!" are set.
    /// 
    /// var mySettings = new SettingsLoader&lt;MySettings&gt;()
    ///     .UseEnvironmentVariables("FOO")
    ///     .UseJson("settings.json")
    ///     .LoadSettings();
    /// 
    /// Assert.That(mySettings.SomeApiKey, Is.EqualTo("api-key-from-env"), "Environment takes precedence");
    /// Assert.That(mySettings.SomeOtherApiKey, Is.EqualTo("from json"));
    /// Assert.That(mySettings.MaxRetryCount, Is.EqualTo(10));
    /// Assert.That(mySettings.FieldsWorkToo, Is.EqualTo("Sure do!"), "Fields never get set")
    /// </code>
    /// 
    /// </example>
    /// <typeparam name="TSettings">The class used to hold the settings.</typeparam>
    public class SettingsLoader<TSettings>
    {
        private readonly ILogger _log = Logging.Logger<SettingsLoader<TSettings>>();

        private string _jsonFile = null;
        private string _envPrefix = null;

        /// <summary>
        /// Sets the path (relative or absolute) to a JSON file from which settings will be loaded.
        /// </summary>
        /// <param name="filename">Must be a valid and non-null path.</param>
        /// <returns>This object for method chaining.</returns>
        public SettingsLoader<TSettings> UseJson(string filename = "secrets.json")
        {
            _jsonFile = Args.NotNull(filename, nameof(filename));
            return this;
        }

        /// <summary>
        /// Sets a prefix used to identify environment variables used for these settings.
        /// </summary>
        /// <example>
        /// If the prefix is <code>FOO_</code>, an environment variable named 
        /// <code>FOO_HELLO_WORLD</code> will map to a settings field named <code>HelloWorld</code>.
        /// </example>
        /// <param name="prefix">A non-null string</param>
        /// <returns>This object, for method chaining</returns>
        public SettingsLoader<TSettings> UseEnvironmentVariables(string prefix = "LP_")
        {
            _envPrefix = Args.NotNull(prefix, nameof(prefix));
            return this;
        }

        /// <summary>
        /// Loads the settings object based on the values set using <see cref="UseJson(string)"/>
        /// and <see cref="UseEnvironmentVariables(string)"/>
        /// </summary>
        /// <remarks>
        /// This method does not cache settings, so calling it twice will load and resolve the settings
        /// twice.
        /// </remarks>
        /// <returns>The loaded settings object</returns>
        public TSettings LoadSettings()
        {
            if (_envPrefix == null && _jsonFile == null)
                _log.LogWarning("Neither UseJson nor UseEnvironmentVariables was called.\n" +
                                $"This will just return a default value of {typeof(TSettings).Name}.");

            var baseSettings = LoadBaseSettingsJson();

            MergeEnvironmentSettings(baseSettings);

            return baseSettings;
        }

        private TSettings LoadBaseSettingsJson()
        {
            if (_jsonFile != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<TSettings>(File.ReadAllText(_jsonFile));
                }
                catch (IOException e)
                {
                    _log.LogDebug($"IOException while loading settings from {_jsonFile}: {e.Message}");
                }
            }

            return Activator.CreateInstance<TSettings>();
        }

        private void MergeEnvironmentSettings(TSettings into)
        {
            if (_envPrefix == null)
                return;

            var env = Environment.GetEnvironmentVariables();
            var matching = env.Keys.OfType<string>().Where(k => k.StartsWith(_envPrefix))
                .Select(k => (ScreamingSnakeToCamelCase(k.Substring(_envPrefix.Length)), env[k]));

            foreach (var pair in matching)
            {
                var (name, value) = pair;

                var access = Reflect.GetAccessor<TSettings>(name);
                // ReSharper disable once UseNullPropagation
                if (access != null)
                {
                    access.SetValue(into, Convert.ChangeType(value, access.GetValueType()));
                }
            }
        }

        private static string ScreamingSnakeToCamelCase(string name)
        {
            return string.Join("", name.Split('_').Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1).ToLower()));
        }

    }
}
