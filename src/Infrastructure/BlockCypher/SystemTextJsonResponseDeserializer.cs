using System.Net.Http;
using System.Text.Json;
using RestEase;

namespace ICMarkets.Infrastructure.BlockCypher;

/// <summary>RestEase deserializer based on System.Text.Json (RestEase uses Newtonsoft.Json by default).</summary>
public class SystemTextJsonResponseDeserializer : ResponseDeserializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonResponseDeserializer(JsonSerializerOptions options = null)
    {
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public override T Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info) =>
        JsonSerializer.Deserialize<T>(content, _options);
}
