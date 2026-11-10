using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datatrans.Net.Serialization;

/// <summary>
/// Maps enums to snake_case strings and falls back to the default member on unknown values,
/// so a value added by Datatrans never breaks deserialization
/// </summary>
internal sealed class TolerantEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(TolerantEnumConverter<>).MakeGenericType(typeToConvert))!;

    internal static string ToSnakeCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
                builder.Append('_');

            builder.Append(char.ToLowerInvariant(name[i]));
        }

        return builder.ToString();
    }

    private sealed class TolerantEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private readonly Dictionary<string, T> _byName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<T, string> _byValue = new();

        public TolerantEnumConverter()
        {
            foreach (var value in Enum.GetValues<T>())
            {
                var name = ToSnakeCase(value.ToString());
                _byName[name] = value;
                _byValue[value] = name;
            }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                reader.Skip();
                return default;
            }

            var raw = reader.GetString();
            return raw != null && _byName.TryGetValue(raw, out var value) ? value : default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(_byValue.TryGetValue(value, out var name) ? name : value.ToString());
    }
}
