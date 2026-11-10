using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datatrans.Net.Models;

/// <summary>Body of <c>POST /v1/transactions/authorize</c>, a payment without the payment page</summary>
public sealed class AuthorizeRequest
{
    /// <summary>Amount in the smallest currency unit, 1000 for CHF 10.00</summary>
    public required long Amount { get; set; }

    /// <summary>ISO 4217 currency code, 3 letters</summary>
    public required string Currency { get; set; }

    /// <summary>Merchant reference, unique per transaction, 40 characters max</summary>
    public required string Refno { get; set; }

    /// <summary>Optional customer reference, 40 characters max</summary>
    public string? Refno2 { get; set; }

    /// <summary>Customer details, for payment methods that support them</summary>
    public CustomerRequest? Customer { get; set; }

    /// <summary>Order with its articles, for payment methods that support it</summary>
    public Order? Order { get; set; }

    /// <summary>Settle right after the authorization, merchant configuration applies when null</summary>
    public bool? AutoSettle { get; set; }

    /// <summary>Card alias to charge</summary>
    public AliasCard? Card { get; set; }

    /// <summary>Twint alias to charge</summary>
    [JsonPropertyName("TWI")]
    public TwintAlias? Twint { get; set; }

    /// <summary>Merchant reference data</summary>
    public Metadata? Metadata { get; set; }

    /// <summary>Properties not modeled here, sent as is, for example <c>PFC</c> or <c>PAP</c></summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}

/// <summary>Card alias used to authorize a payment</summary>
public sealed class AliasCard
{
    /// <summary>Card kind, always <c>ALIAS</c></summary>
    public string Type => "ALIAS";

    /// <summary>Alias received from a previous transaction made with <c>option.createAlias</c></summary>
    public required string Alias { get; set; }

    /// <summary>Expiry month, 2 digits, can be omitted when stored with the alias</summary>
    public string? ExpiryMonth { get; set; }

    /// <summary>Expiry year, 2 digits, can be omitted when stored with the alias</summary>
    public string? ExpiryYear { get; set; }
}

/// <summary>Twint alias used to authorize a payment</summary>
public sealed class TwintAlias
{
    /// <summary>Alias received from a previous transaction made with <c>option.createAlias</c></summary>
    public required string Alias { get; set; }
}

/// <summary>Body of <c>POST /v1/transactions/validate</c></summary>
public sealed class ValidateRequest
{
    /// <summary>Merchant reference, unique per transaction, 40 characters max</summary>
    public required string Refno { get; set; }

    /// <summary>Optional customer reference, 40 characters max</summary>
    public string? Refno2 { get; set; }

    /// <summary>ISO 4217 currency code, 3 letters</summary>
    public required string Currency { get; set; }

    /// <summary>Card alias to validate</summary>
    public ValidateCard? Card { get; set; }

    /// <summary>Properties not modeled here, sent as is, for example <c>PFC</c> or <c>PAP</c></summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}

/// <summary>Card alias to validate</summary>
public sealed class ValidateCard
{
    /// <summary>Alias received from a previous transaction made with <c>option.createAlias</c></summary>
    public required string Alias { get; set; }

    /// <summary>Expiry month, 2 digits, can be omitted when stored with the alias</summary>
    public string? ExpiryMonth { get; set; }

    /// <summary>Expiry year, 2 digits, can be omitted when stored with the alias</summary>
    public string? ExpiryYear { get; set; }
}

/// <summary>Response of the authorize and validate endpoints</summary>
public sealed class AuthorizeResponse
{
    /// <summary>Id to use for the next actions, like a settlement</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>Authorization code returned by the acquirer</summary>
    public string? AcquirerAuthorizationCode { get; set; }

    /// <summary>Card used, alias and masked number only</summary>
    public CardDetail? Card { get; set; }

    /// <summary>Properties not modeled here</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
