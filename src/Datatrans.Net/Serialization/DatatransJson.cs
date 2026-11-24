using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datatrans.Net.Serialization;

/// <summary>JSON settings shared by requests, responses and webhooks</summary>
public static class DatatransJson
{
    /// <summary>
    /// Serializer options matching the Datatrans API conventions, read-only,
    /// copy them with <c>new JsonSerializerOptions(DatatransJson.Options)</c> to customize
    /// </summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            // The API rejects null and unknown properties
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new TolerantEnumConverterFactory());

        // Shared by the client, a change from outside would break every call
        options.MakeReadOnly(populateMissingResolver: true);

        return options;
    }
}
