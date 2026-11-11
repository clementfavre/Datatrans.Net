namespace Datatrans.Net.Models;

/// <summary>Alias of a payment method, received from a previous transaction made with <c>option.createAlias</c></summary>
public sealed class PaymentMethodAlias
{
    /// <summary>Alias to reuse</summary>
    public string? Alias { get; set; }
}

/// <summary>PayPal parameters of an init request</summary>
public sealed class PayPalInit
{
    /// <summary>Alias to reuse</summary>
    public string? Alias { get; set; }

    /// <summary>HTTPS URL of the merchant logo</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Whether the shipping details are displayed</summary>
    public bool? DisplayShippingDetails { get; set; }

    /// <summary>Forward the customer details to PayPal</summary>
    public bool? ForwardCustomerDetails { get; set; }

    /// <summary>Retrieve the customer details from PayPal</summary>
    public bool? ReturnCustomerDetails { get; set; }

    /// <summary>Create a PayPal order instead of a direct payment</summary>
    public bool? CreateOrder { get; set; }

    /// <summary>PayPal FraudNet session identifier</summary>
    public string? FraudSessionId { get; set; }
}

/// <summary>PayPal parameters of an authorize request</summary>
public sealed class PayPalAuthorize
{
    /// <summary>Alias to charge</summary>
    public string? Alias { get; set; }

    /// <summary>Id of the order transaction, when the authorization follows a PayPal order</summary>
    public string? OrderTransactionId { get; set; }

    /// <summary>PayPal FraudNet session identifier</summary>
    public string? FraudSessionId { get; set; }
}

/// <summary>PostFinance Pay parameters of an init request</summary>
public sealed class PostFinancePayInit
{
    /// <summary>Alias to reuse</summary>
    public string? Alias { get; set; }

    /// <summary>Step up flag, see the Datatrans documentation</summary>
    public bool? StepUp { get; set; }
}

/// <summary>PayPal details of a transaction</summary>
public sealed class PayPalDetail
{
#pragma warning disable CS1591
    /// <summary>PayPal order id, when the transaction was a PayPal order</summary>
    public string? OrderId { get; set; }

    public string? Alias { get; set; }
    public string? PayerId { get; set; }
#pragma warning restore CS1591
}

/// <summary>PostFinance Card details of a transaction</summary>
public sealed class PostFinanceCardDetail
{
#pragma warning disable CS1591
    public string? Alias { get; set; }

    /// <summary>Masked card number, for example <c>****2345</c></summary>
    public string? Masked { get; set; }

    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
#pragma warning restore CS1591
}

/// <summary>PostFinance Pay details of a transaction</summary>
public sealed class PostFinancePayDetail
{
#pragma warning disable CS1591
    public string? Alias { get; set; }
    public string? MaskedIban { get; set; }
    public string? Fingerprint { get; set; }
#pragma warning restore CS1591
}

/// <summary>Billing address of the customer</summary>
public sealed class BillingAddress
{
#pragma warning disable CS1591
    public string? Gender { get; set; }
    public string? Title { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Street { get; set; }
    public string? Street2 { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }

    /// <summary>ISO 3166-1 alpha-3 country code, for example <c>CHE</c></summary>
    public string? Country { get; set; }

    public string? CountrySubdivision { get; set; }
    public string? SortingCode { get; set; }
    public string? PhoneNumber { get; set; }
#pragma warning restore CS1591
}

/// <summary>Address the articles are sent to</summary>
public sealed class ShippingAddress
{
#pragma warning disable CS1591
    public string? Gender { get; set; }
    public string? Title { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Street { get; set; }
    public string? Street2 { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }

    /// <summary>ISO 3166-1 alpha-3 country code, for example <c>CHE</c></summary>
    public string? Country { get; set; }

    public string? CountrySubdivision { get; set; }
    public string? SortingCode { get; set; }
    public string? Phone { get; set; }
    public string? CellPhone { get; set; }

    /// <summary>Shipping carrier, for example <c>DHL</c></summary>
    public string? Carrier { get; set; }

    /// <summary>Value of all items in the basket, smallest currency unit</summary>
    public long? Price { get; set; }

    /// <summary>Gross value of all items in the basket, smallest currency unit</summary>
    public long? PriceGross { get; set; }
#pragma warning restore CS1591
}

/// <summary>Partial settlement of an authorized amount</summary>
public sealed class PartialCapture
{
    /// <summary>Number of this partial settlement, incremented on each request</summary>
    public required int SequenceNumber { get; set; }

    /// <summary>Total number of expected partial settlements</summary>
    public required int TotalNumber { get; set; }
}

/// <summary>Body of <c>POST /v1/transactions/{transactionId}/authorize</c></summary>
public sealed class AuthorizeAuthenticatedRequest
{
    /// <summary>Amount in the smallest currency unit</summary>
    public long? Amount { get; set; }

    /// <summary>Merchant reference, 40 characters max</summary>
    public required string Refno { get; set; }

    /// <summary>Optional customer reference, 40 characters max</summary>
    public string? Refno2 { get; set; }

    /// <summary>Settle right after the authorization, merchant configuration applies when null</summary>
    public bool? AutoSettle { get; set; }

    /// <summary>Customer details, for payment methods that support them</summary>
    public CustomerRequest? Customer { get; set; }

    /// <summary>Properties not modeled here, sent as is</summary>
    [System.Text.Json.Serialization.JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}

/// <summary>Response of <c>POST /v1/transactions/{transactionId}/authorize</c></summary>
public sealed class AuthorizeAuthenticatedResponse
{
    /// <summary>Authorization code returned by the acquirer</summary>
    public string? AcquirerAuthorizationCode { get; set; }
}

/// <summary>Body of <c>POST /v1/transactions/{transactionId}/increase</c></summary>
public sealed class IncreaseRequest
{
    /// <summary>Amount in the smallest currency unit</summary>
    public required long Amount { get; set; }

    /// <summary>ISO 4217 currency code, 3 letters</summary>
    public required string Currency { get; set; }

    /// <summary>Merchant reference, 40 characters max</summary>
    public required string Refno { get; set; }

    /// <summary>Merchant reference data</summary>
    public Metadata? Metadata { get; set; }
}

/// <summary>Response of <c>POST /v1/transactions/{transactionId}/increase</c></summary>
public sealed class IncreaseResponse
{
    /// <summary>Total authorized amount, initial authorization and increases included</summary>
    public long IncreasedAmount { get; set; }
}

/// <summary>Body of <c>PATCH /v1/aliases/{alias}</c></summary>
public sealed class AliasPatchRequest
{
    /// <summary>Remove the card number from storage</summary>
    public bool? RemovePlain { get; set; }

    /// <summary>New expiry month, 2 digits</summary>
    public string? ExpiryMonth { get; set; }

    /// <summary>New expiry year, 2 digits</summary>
    public string? ExpiryYear { get; set; }

    /// <summary>Create a network token for the alias</summary>
    public bool? CreateNetworkToken { get; set; }

    /// <summary>Properties not modeled here, sent as is</summary>
    [System.Text.Json.Serialization.JsonExtensionData]
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}
