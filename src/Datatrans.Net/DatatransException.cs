using System.Net;

namespace Datatrans.Net;

/// <summary>Thrown when the Datatrans API answers with a non success status code</summary>
public sealed class DatatransException : Exception
{
    /// <summary>Creates the exception</summary>
    public DatatransException(HttpStatusCode statusCode, DatatransErrorCode errorCode, string? rawErrorCode,
        string message, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        RawErrorCode = rawErrorCode;
        ResponseBody = responseBody;
    }

    /// <summary>HTTP status code of the response</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Parsed error code, <see cref="DatatransErrorCode.Unknown"/> when not recognized</summary>
    public DatatransErrorCode ErrorCode { get; }

    /// <summary>Error code exactly as returned by the API</summary>
    public string? RawErrorCode { get; }

    /// <summary>Raw response body</summary>
    public string? ResponseBody { get; }
}

/// <summary>Error codes returned by the transactions and aliases APIs</summary>
public enum DatatransErrorCode
{
#pragma warning disable CS1591
    Unknown = 0,
    InvalidSetup,
    ExpiredCard,
    ClientError,
    InvalidTransactionStatus,
    Referral,
    ConcurrentLimitReached,
    RoutingNotFound,
    BlockedCard,
    InvalidCvv,
    UnknownError,
    IdempotencyPayloadMismatch,
    UnrecognizedProperty,
    ServerError,
    Unauthorized,
    IdempotencyRequestOutstanding,
    NotSupportedByThirdParty,
    PaymentReferenceIdExist,
    RoutingNotConfigured,
    BlockedByVelocityChecker,
    RoutingLookupFailed,
    NotFound,
    ThirdPartyError,
    InvalidJsonPayload,
    InvalidSign,
    UnsupportedCard,
    Declined,
    Forbidden,
    InvalidAlias,
    InvalidState,
    UnsupportedPaymentMethod,
    InvalidProperty,
    InvalidHeader,
    SoftDeclined,
    RequestIdExist,
    DuplicateRefno,
    TransactionNotFound,
    InvalidCard,

    // Aliases API
    PlainNotFound,
    InvalidExpiration,
    InvalidFingerprint,
    IllegalArgument,
    CouldNotObtainNetworkToken,
    InvalidPlainValueForTestMode,
    MaxRequestsPerCallExceeded,
    VelocityError,
    InvalidPlainValue,
    NotVerified,
    FingerprintNotFound,
    InternalError,
    InvalidConfiguration,
    CouldNotDecryptToken,
    AliasNotFound,
    CouldNotDeleteNetworkToken,
    TokenProviderUnavailable,
    CouldNotObtainCardArt,
    DeleteNotAllowed,
    ServiceNotAllowed
#pragma warning restore CS1591
}
