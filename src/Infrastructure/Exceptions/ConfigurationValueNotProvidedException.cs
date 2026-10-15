using System;

namespace ICMarkets.Infrastructure.Exceptions;

/// <summary>Thrown when a required configuration value is missing or empty.</summary>
public class ConfigurationValueNotProvidedException : Exception
{
    public string ConfigName { get; }
    public string ValueName { get; }

    public ConfigurationValueNotProvidedException(string configName, string valueName) :
        base($"Configuration value '{configName}:{valueName}' is required but was not provided.")
    {
        ConfigName = configName;
        ValueName = valueName;
    }
}
