using System;

namespace ICMarkets.API.Options;

/// <summary>"Cors" section of the configuration. An empty list allows nothing of that kind.</summary>
public class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();

    /// <summary>Response headers the browser lets client scripts read, e.g. X-Correlation-ID.</summary>
    public string[] ExposedHeaders { get; set; } = Array.Empty<string>();
}
