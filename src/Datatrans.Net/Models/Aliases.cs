using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datatrans.Net.Models;

/// <summary>Response of <c>GET /v1/aliases/{alias}</c></summary>
public sealed class AliasInfoResponse
{
#pragma warning disable CS1591
    public string Alias { get; set; } = string.Empty;
    public string? Fingerprint { get; set; }

    /// <summary>Alias kind, for example <c>CARD</c>, <c>TWI</c> or <c>PFC</c></summary>
    public string? Type { get; set; }

    /// <summary>Masked value behind the alias, for example <c>490000xxxxxx0003</c></summary>
    public string? Masked { get; set; }

    public DateTimeOffset? DateCreated { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
#pragma warning restore CS1591

    /// <summary>Card behind the alias, when available</summary>
    public AliasCardInfo? Card { get; set; }

    /// <summary>Properties not modeled here</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Card behind an alias</summary>
public sealed class AliasCardInfo
{
#pragma warning disable CS1591
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Last4 { get; set; }
    public string? Bin { get; set; }
    public bool? PanRemoved { get; set; }
    public CardInfo? CardInfo { get; set; }

    /// <summary>Wallet code when the card came from a wallet, for example <c>APL</c></summary>
    public string? WalletIndicator { get; set; }
#pragma warning restore CS1591
}

/// <summary>Brand and issuer of a card</summary>
public sealed class CardInfo
{
#pragma warning disable CS1591
    /// <summary>Brand, for example <c>VISA CREDIT</c></summary>
    public string? Brand { get; set; }

    /// <summary><c>credit</c>, <c>debit</c> or <c>prepaid</c></summary>
    public string? Type { get; set; }

    /// <summary><c>consumer</c>, <c>corporate</c> or <c>unknown</c></summary>
    public string? Usage { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code</summary>
    public string? Country { get; set; }

    public string? Issuer { get; set; }
#pragma warning restore CS1591
}
