using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Datatrans.Net.Models;
using Datatrans.Net.Serialization;

namespace Datatrans.Net;

/// <summary>Helpers to verify and read the webhook sent by Datatrans after a payment</summary>
public static class DatatransWebhook
{
    /// <summary>Name of the header carrying the signature</summary>
    public const string SignatureHeaderName = "Datatrans-Signature";

    /// <summary>
    /// Verifies the <c>Datatrans-Signature</c> header against the raw request body
    /// </summary>
    /// <param name="signatureHeader">Header value, <c>t={timestamp},s0={signature}</c></param>
    /// <param name="rawBody">Request body exactly as received, not reformatted</param>
    /// <param name="hmacKey">Hex HMAC key from the merchant security settings</param>
    /// <param name="tolerance">When set, rejects a timestamp further than this from now</param>
    public static bool IsSignatureValid(string? signatureHeader, string rawBody, string hmacKey,
        TimeSpan? tolerance = null)
    {
        ArgumentNullException.ThrowIfNull(rawBody);
        ArgumentException.ThrowIfNullOrWhiteSpace(hmacKey);

        if (!TryParseHeader(signatureHeader, out var timestamp, out var signature))
            return false;

        if (tolerance.HasValue && !IsTimestampFresh(timestamp, tolerance.Value))
            return false;

        byte[] expected;
        try
        {
            expected = Convert.FromHexString(signature);
        }
        catch (FormatException)
        {
            return false;
        }

        var computed = HMACSHA256.HashData(Convert.FromHexString(hmacKey),
            Encoding.UTF8.GetBytes(timestamp + rawBody));

        return CryptographicOperations.FixedTimeEquals(computed, expected);
    }

    /// <summary>Reads the webhook payload, same shape as a status response</summary>
    public static StatusResponse Parse(string rawBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawBody);

        return JsonSerializer.Deserialize<StatusResponse>(rawBody, DatatransJson.Options)
               ?? throw new JsonException("Empty webhook payload");
    }

    private static bool TryParseHeader(string? header, out string timestamp, out string signature)
    {
        timestamp = string.Empty;
        signature = string.Empty;

        if (string.IsNullOrWhiteSpace(header))
            return false;

        foreach (var part in header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = part[..separator];
            var value = part[(separator + 1)..];
            if (key == "t")
                timestamp = value;
            else if (key == "s0")
                signature = value;
        }

        return timestamp.Length > 0 && signature.Length > 0;
    }

    private static bool IsTimestampFresh(string timestamp, TimeSpan tolerance)
    {
        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds))
            return false;

        try
        {
            var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return age.Duration() <= tolerance;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
