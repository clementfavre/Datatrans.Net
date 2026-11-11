using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datatrans.Net.Models;

/// <summary>Response of <c>POST /v1/transactions</c></summary>
public sealed class InitResponse
{
    /// <summary>Id to start the payment page with, valid for 30 minutes</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>Token for the mobile SDKs, only when requested</summary>
    public string? MobileToken { get; set; }

    /// <summary>URL of the payment page to redirect the customer to</summary>
    [JsonIgnore]
    public Uri? PaymentPageUrl { get; set; }

    /// <summary>Properties not modeled here</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Response of <c>POST /v1/transactions/{transactionId}/credit</c></summary>
public sealed class CreditResponse
{
    /// <summary>Id of the new transaction created by the refund</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>Authorization code returned by the acquirer</summary>
    public string? AcquirerAuthorizationCode { get; set; }
}

/// <summary>
/// Response of <c>GET /v1/transactions/{transactionId}</c>, also the payload of a webhook
/// </summary>
public sealed class StatusResponse
{
#pragma warning disable CS1591
    public string TransactionId { get; set; } = string.Empty;
    public string? MerchantId { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public string? Currency { get; set; }
    public string? Refno { get; set; }
    public string? Refno2 { get; set; }
    public Metadata? Metadata { get; set; }
#pragma warning restore CS1591

    /// <summary>Payment method code, see <see cref="PaymentMethods"/></summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Details of each step of the transaction</summary>
    public TransactionDetail? Detail { get; set; }

    /// <summary>Customer details, when collected by the payment page</summary>
    public Customer? Customer { get; set; }

    /// <summary>Language the payment page was shown in</summary>
    public string? Language { get; set; }

    /// <summary>Card details, when a card was used</summary>
    public CardDetail? Card { get; set; }

    /// <summary>Twint details, when Twint was used</summary>
    [JsonPropertyName("TWI")]
    public TwintDetail? Twint { get; set; }

    /// <summary>PayPal details, when PayPal was used</summary>
    [JsonPropertyName("PAP")]
    public PayPalDetail? PayPal { get; set; }

    /// <summary>PostFinance Card details, when PostFinance Card was used</summary>
    [JsonPropertyName("PFC")]
    public PostFinanceCardDetail? PostFinanceCard { get; set; }

    /// <summary>PostFinance Pay details, when PostFinance Pay was used</summary>
    [JsonPropertyName("PFP")]
    public PostFinancePayDetail? PostFinancePay { get; set; }

    /// <summary>Actions performed on the transaction</summary>
    public IList<TransactionAction>? History { get; set; }

    /// <summary>Properties not modeled here, for example <c>KLN</c> or <c>REK</c></summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }

    /// <summary>True when the payment went through, authorized or already settled</summary>
    [JsonIgnore]
    public bool IsPaid => Status is TransactionStatus.Authorized or TransactionStatus.Settled
        or TransactionStatus.Transmitted;
}

/// <summary>Kind of transaction</summary>
public enum TransactionType
{
#pragma warning disable CS1591
    Unknown = 0,
    Payment,
    Credit,
    CardCheck
#pragma warning restore CS1591
}

/// <summary>Status of a transaction</summary>
public enum TransactionStatus
{
#pragma warning disable CS1591
    Unknown = 0,
    Initialized,
    ChallengeRequired,
    ChallengeOngoing,
    Authenticated,
    Authorized,
    Settled,
    Canceled,
    Transmitted,
    Failed
#pragma warning restore CS1591
}

/// <summary>Reason of a failed transaction</summary>
public enum FailReason
{
#pragma warning disable CS1591
    Unknown = 0,
    CardBlocked,
    CardInvalid,
    Declined,
    SoftDeclined,
    Incomplete,
    Timeout,
    InvalidSetup,
    SecureAuthentication,
    ErrorOnStart,
    Referral,
    RoutingNotFound,
    Error
#pragma warning restore CS1591
}

/// <summary>Details of the steps of a transaction, a step is null when it did not happen</summary>
public sealed class TransactionDetail
{
#pragma warning disable CS1591
    public InitDetail? Init { get; set; }
    public AuthorizeDetail? Authorize { get; set; }
    public AmountDetail? Settle { get; set; }
    public AmountDetail? Credit { get; set; }
    public CancelDetail? Cancel { get; set; }
    public FailDetail? Fail { get; set; }
#pragma warning restore CS1591
}

/// <summary>Init step</summary>
public sealed class InitDetail
{
    /// <summary>When the initialized transaction expires if not continued</summary>
    public DateTimeOffset? Expires { get; set; }
}

/// <summary>Authorize step</summary>
public sealed class AuthorizeDetail
{
    /// <summary>Authorized amount in the smallest currency unit</summary>
    public long? Amount { get; set; }

    /// <summary>Authorization code returned by the acquirer</summary>
    public string? AcquirerAuthorizationCode { get; set; }
}

/// <summary>Settle or credit step</summary>
public sealed class AmountDetail
{
    /// <summary>Amount in the smallest currency unit</summary>
    public long? Amount { get; set; }
}

/// <summary>Cancel step</summary>
public sealed class CancelDetail
{
    /// <summary>Whether the transaction was reversed on the acquirer side</summary>
    public bool? Reversal { get; set; }
}

/// <summary>Fail step</summary>
public sealed class FailDetail
{
    /// <summary>Failure reason</summary>
    public FailReason Reason { get; set; }

    /// <summary>Detailed error message</summary>
    public string? Message { get; set; }
}

/// <summary>Card used for a transaction</summary>
public sealed class CardDetail
{
#pragma warning disable CS1591
    public string? Alias { get; set; }
    public string? Fingerprint { get; set; }

    /// <summary>Masked card number, for example <c>424242xxxxxx4242</c></summary>
    public string? Masked { get; set; }

    public string? Bin { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }

    /// <summary>Wallet code when the card came from a wallet, for example <c>APL</c></summary>
    public string? WalletIndicator { get; set; }
#pragma warning restore CS1591
}

/// <summary>Twint details of a transaction</summary>
public sealed class TwintDetail
{
#pragma warning disable CS1591
    public string? Alias { get; set; }
    public string? Fingerprint { get; set; }
#pragma warning restore CS1591
}

/// <summary>Customer details returned with a status</summary>
public sealed class Customer
{
#pragma warning disable CS1591
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Street { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public string? BirthDate { get; set; }
    public string? Type { get; set; }
#pragma warning restore CS1591
}

/// <summary>One entry of the transaction history</summary>
public sealed class TransactionAction
{
    /// <summary>Action performed, for example <c>init</c>, <c>authorize</c>, <c>settle</c></summary>
    public string? Action { get; set; }

    /// <summary>Whether auto settlement was used, only for <c>authorize</c></summary>
    public bool? AutoSettle { get; set; }

    /// <summary>Id of the action on the Datatrans side</summary>
    public string? RequestId { get; set; }

    /// <summary>Amount in the smallest currency unit</summary>
    public long? Amount { get; set; }

    /// <summary>Where the action came from, for example <c>api</c> or <c>redirect</c></summary>
    public string? Source { get; set; }

    /// <summary>When the action was performed</summary>
    public DateTimeOffset? Date { get; set; }

    /// <summary>Whether the action succeeded</summary>
    public bool? Success { get; set; }

    /// <summary>IP address the action came from</summary>
    public string? Ip { get; set; }
}
