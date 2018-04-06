# CSharp_Utils

This is a library of little tidbits we found useful around the QBE Lab. Mainly helps with JSON, loading settings, etc.

## Installing

You can clone and add this project as a dependancy to yours, or install it as a package via Nuget:

```nuget
Install-Package QBE.LabUtilities
```

## Library Classes

### Args

Allows you to test to see if a function argument is null.

### JsonPathConverter

Enables the use of paths to properties in JsonProperty attributes:

```c#
class Person {
    [JsonProperty("address.country")]
    public string Country {get; set;}
}

var person = JsonConvert.DeserializeObject&lt;Person&gt;(
    @"{
        ""address"": {
            ""country"": ""Norway""
        }
    }");

person.Country // => "Norway"
```

### Logging

Provides a generic logging class that provides a unified front for an arbitrary logging interface, allowing the user to switch loggers without changing a ton of code.

### Reflect

Represents a field or property that we can read/write.

### SettingsLoader

Provides a generic way to load settings from JSON files and environment variables.

The JSON and environment settings are used according to the following semantics:

*   Values present in the environment will overwrite those found in the settings file.
*   If the JSON file does not exist, and message will be logged at the `LogLevel.Trace` severity, and the resulting object's properties are left as their default values.
*   Environment variables that start with the given prefix are converted from `SCREAMING_SNAKE_CASE` to `UpperCamelCase` after stripping the prefix to map them to corresponding fields.
