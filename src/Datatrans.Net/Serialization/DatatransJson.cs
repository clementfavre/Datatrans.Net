using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datatrans.Net.Serialization;

/// <summary>JSON settings shared by requests, responses and webhooks</summary>
public static class DatatransJson
{
    /// <summary>Serializer options matching the Datatrans API conventions</summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            // The API rejects null and unknown properties
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new TolerantEnumConverterFactory());

        return options;
    }
}
