namespace Datatrans.Net;

/// <summary>Datatrans environments</summary>
public enum DatatransEnvironment
{
    /// <summary>Test environment, no real money involved</summary>
    Sandbox,

    /// <summary>Live environment</summary>
    Production
}

/// <summary>Settings used by <see cref="DatatransClient"/></summary>
public sealed class DatatransOptions
{
    private const string SandboxApiBaseUrl = "https://api.sandbox.datatrans.com";
    private const string ProductionApiBaseUrl = "https://api.datatrans.com";
    private const string SandboxPaymentPageBaseUrl = "https://pay.sandbox.datatrans.com";
    private const string ProductionPaymentPageBaseUrl = "https://pay.datatrans.com";

    /// <summary>Merchant id, used as the basic auth username</summary>
    public string MerchantId { get; set; } = string.Empty;

    /// <summary>Server-to-server password from the merchant security settings</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Environment to call, sandbox by default</summary>
    public DatatransEnvironment Environment { get; set; } = DatatransEnvironment.Sandbox;

    /// <summary>Hex HMAC key used to verify webhook signatures, optional</summary>
    public string? HmacKey { get; set; }

    /// <summary>Overrides the API base URL of the selected environment</summary>
    public string? ApiBaseUrl { get; set; }

    /// <summary>Overrides the payment page base URL of the selected environment</summary>
    public string? PaymentPageBaseUrl { get; set; }

    internal string ResolveApiBaseUrl() => Resolve(ApiBaseUrl, SandboxApiBaseUrl, ProductionApiBaseUrl);

    internal string ResolvePaymentPageBaseUrl() =>
        Resolve(PaymentPageBaseUrl, SandboxPaymentPageBaseUrl, ProductionPaymentPageBaseUrl);

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(MerchantId))
            throw new ArgumentException("MerchantId is required", nameof(MerchantId));

        if (string.IsNullOrWhiteSpace(Password))
            throw new ArgumentException("Password is required", nameof(Password));
    }

    private string Resolve(string? overrideUrl, string sandbox, string production)
    {
        if (!string.IsNullOrWhiteSpace(overrideUrl))
            return overrideUrl.TrimEnd('/');

        return Environment == DatatransEnvironment.Production ? production : sandbox;
    }
}
