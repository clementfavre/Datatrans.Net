using System.Text.Json.Serialization;

namespace Datatrans.Net.Models;

/// <summary>Body of <c>POST /v1/transactions</c></summary>
public sealed class InitRequest
{
    /// <summary>Amount in the smallest currency unit, 1000 for CHF 10.00</summary>
    public long? Amount { get; set; }

    /// <summary>ISO 4217 currency code, 3 letters</summary>
    public required string Currency { get; set; }

    /// <summary>Merchant reference, unique per transaction, 40 characters max</summary>
    public required string Refno { get; set; }

    /// <summary>Optional customer reference, 40 characters max</summary>
    public string? Refno2 { get; set; }

    /// <summary>Customer details, for payment methods that support them</summary>
    public CustomerRequest? Customer { get; set; }

    /// <summary>Settle right after the authorization, merchant configuration applies when null</summary>
    public bool? AutoSettle { get; set; }

    /// <summary>Additional options</summary>
    public InitOptions? Option { get; set; }

    /// <summary>Payment page language, 2 letters</summary>
    public string? Language { get; set; }

    /// <summary>Browser redirect URLs</summary>
    public RedirectRequest? Redirect { get; set; }

    /// <summary>Webhook override, merchant configuration applies when null</summary>
    public WebhookRequest? Webhook { get; set; }

    /// <summary>Payment method codes to offer, see <see cref="PaymentMethods"/>, all available when null</summary>
    public IList<string>? PaymentMethods { get; set; }

    /// <summary>Merchant reference data</summary>
    public Metadata? Metadata { get; set; }

    /// <summary>Properties not modeled here, sent as is, for example <c>TWI</c> or <c>theme</c></summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}

/// <summary>Options of an init request</summary>
public sealed class InitOptions
{
    /// <summary>Create an alias for this transaction</summary>
    public bool? CreateAlias { get; set; }

    /// <summary>Return the masked card number</summary>
    public bool? ReturnMaskedCardNumber { get; set; }

    /// <summary>Return the country of the customer's issuer</summary>
    public bool? ReturnCustomerCountry { get; set; }

    /// <summary>Only authenticate (3D process), no authorization</summary>
    public bool? AuthenticationOnly { get; set; }

    /// <summary>Store the customer object with the transaction</summary>
    public bool? StoreCustomerData { get; set; }
}

/// <summary>Browser behaviour on the payment page</summary>
public sealed class RedirectRequest
{
    /// <summary>URL the customer lands on after a successful transaction</summary>
    public string? SuccessUrl { get; set; }

    /// <summary>URL the customer lands on after canceling</summary>
    public string? CancelUrl { get; set; }

    /// <summary>URL the customer lands on after an error</summary>
    public string? ErrorUrl { get; set; }

    /// <summary>Use <c>_top</c> when the payment starts in an iframe or in Lightbox mode</summary>
    public string? StartTarget { get; set; }

    /// <summary>Use <c>_top</c> to open the redirect URLs outside of the iframe</summary>
    public string? ReturnTarget { get; set; }

    /// <summary>HTTP method of the redirect, <c>GET</c> or <c>POST</c></summary>
    public string? Method { get; set; }
}

/// <summary>Webhook configuration of a single transaction</summary>
public sealed class WebhookRequest
{
    /// <summary>URL the webhook is sent to</summary>
    public required string Url { get; set; }
}

/// <summary>Merchant reference data, 40 characters max each</summary>
public sealed class Metadata
{
#pragma warning disable CS1591
    public string? UserData1 { get; set; }
    public string? UserData2 { get; set; }
    public string? UserData3 { get; set; }
    public string? UserData4 { get; set; }
#pragma warning restore CS1591
}

/// <summary>Customer details sent with an init request</summary>
public sealed class CustomerRequest
{
#pragma warning disable CS1591
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Street { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code</summary>
    public string? Country { get; set; }

    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? CellPhone { get; set; }
    public string? Email { get; set; }

    /// <summary><c>female</c> or <c>male</c></summary>
    public string? Gender { get; set; }

    /// <summary>ISO 8601 date, <c>yyyy-MM-dd</c></summary>
    public string? BirthDate { get; set; }

    public string? Language { get; set; }

    /// <summary><c>P</c> for a private customer, <c>C</c> for a company</summary>
    public string? Type { get; set; }

    /// <summary>Company name, only when <see cref="Type"/> is <c>C</c></summary>
    public string? Name { get; set; }

    public string? CompanyLegalForm { get; set; }
    public string? CompanyRegisterNumber { get; set; }
    public string? IpAddress { get; set; }
#pragma warning restore CS1591
}

/// <summary>Body of <c>POST /v1/transactions/{transactionId}/settle</c></summary>
public sealed class SettleRequest
{
    /// <summary>Amount to settle in the smallest currency unit</summary>
    public required long Amount { get; set; }

    /// <summary>ISO 4217 currency code, 3 letters</summary>
    public required string Currency { get; set; }

    /// <summary>Merchant reference</summary>
    public required string Refno { get; set; }

    /// <summary>Optional customer reference</summary>
    public string? Refno2 { get; set; }

    /// <summary>Properties not modeled here, sent as is</summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}

/// <summary>Body of <c>POST /v1/transactions/{transactionId}/cancel</c></summary>
public sealed class CancelRequest
{
    /// <summary>Properties not modeled here, sent as is</summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}

/// <summary>Body of <c>POST /v1/transactions/{transactionId}/credit</c></summary>
public sealed class CreditRequest
{
    /// <summary>Amount to refund in the smallest currency unit</summary>
    public long? Amount { get; set; }

    /// <summary>ISO 4217 currency code, 3 letters</summary>
    public required string Currency { get; set; }

    /// <summary>Merchant reference, unique per transaction</summary>
    public required string Refno { get; set; }

    /// <summary>Optional customer reference</summary>
    public string? Refno2 { get; set; }

    /// <summary>Merchant reference data</summary>
    public Metadata? Metadata { get; set; }

    /// <summary>Properties not modeled here, sent as is</summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}
