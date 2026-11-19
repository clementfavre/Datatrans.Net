using Datatrans.Net.Models;
using Microsoft.Extensions.Configuration;

namespace Datatrans.Net.IntegrationTests;

/// <summary>Sandbox credentials read from user secrets or environment variables (section <c>Datatrans</c>)</summary>
internal sealed class Sandbox
{
    private Sandbox(string merchantId, string password)
    {
        MerchantId = merchantId;
        Password = password;
    }

    public string MerchantId { get; }

    public string Password { get; }

    /// <summary>Card alias from a sandbox payment made with <c>option.createAlias</c>, optional</summary>
    public string? CardAlias { get; private init; }

    public string? CardExpiryMonth { get; private init; }

    public string? CardExpiryYear { get; private init; }

    /// <summary>Sub-merchant id for the marketplace splits, optional</summary>
    public string? SubMerchantId { get; private init; }

    /// <summary>Null when the merchant id or the password is missing</summary>
    public static Sandbox? Current { get; } = Load();

    public static RedirectRequest Redirect { get; } = new()
    {
        SuccessUrl = "https://example.com/success",
        CancelUrl = "https://example.com/cancel",
        ErrorUrl = "https://example.com/error"
    };

    // Always targets the sandbox, whatever the credentials
    public DatatransClient CreateClient(string? password = null) => new(new DatatransOptions
    {
        MerchantId = MerchantId,
        Password = password ?? Password,
        Environment = DatatransEnvironment.Sandbox
    });

    public AliasCard CreateAliasCard() => new()
    {
        Alias = CardAlias ?? throw new InvalidOperationException("Datatrans:CardAlias is not set"),
        ExpiryMonth = CardExpiryMonth,
        ExpiryYear = CardExpiryYear
    };

    // Unique per run, within the 40 characters limit
    public static string NewRefno() => "it-" + Guid.NewGuid().ToString("N");

    private static Sandbox? Load()
    {
        var section = new ConfigurationBuilder()
            .AddUserSecrets<Sandbox>(optional: true)
            .AddEnvironmentVariables()
            .Build()
            .GetSection("Datatrans");

        var merchantId = section["MerchantId"];
        var password = section["Password"];
        if (string.IsNullOrWhiteSpace(merchantId) || string.IsNullOrWhiteSpace(password))
            return null;

        return new Sandbox(merchantId, password)
        {
            CardAlias = NullIfEmpty(section["CardAlias"]),
            CardExpiryMonth = NullIfEmpty(section["CardExpiryMonth"]),
            CardExpiryYear = NullIfEmpty(section["CardExpiryYear"]),
            SubMerchantId = NullIfEmpty(section["SubMerchantId"])
        };
    }

    // CI passes missing secrets as empty strings
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}

/// <summary>Skipped when the sandbox credentials are missing</summary>
public sealed class SandboxFactAttribute : FactAttribute
{
    public SandboxFactAttribute()
    {
        if (Sandbox.Current is null)
            Skip = "Sandbox credentials missing, set Datatrans:MerchantId and Datatrans:Password";
    }
}

/// <summary>Skipped when the sandbox credentials are missing</summary>
public sealed class SandboxTheoryAttribute : TheoryAttribute
{
    public SandboxTheoryAttribute()
    {
        if (Sandbox.Current is null)
            Skip = "Sandbox credentials missing, set Datatrans:MerchantId and Datatrans:Password";
    }
}

/// <summary>Skipped when the sandbox credentials or the card alias are missing</summary>
public sealed class SandboxAliasFactAttribute : FactAttribute
{
    public SandboxAliasFactAttribute()
    {
        if (Sandbox.Current?.CardAlias is null)
            Skip = "Sandbox card alias missing, set Datatrans:CardAlias, Datatrans:CardExpiryMonth and Datatrans:CardExpiryYear";
    }
}

/// <summary>Skipped when the sandbox credentials or the sub-merchant id are missing</summary>
public sealed class SandboxMarketplaceFactAttribute : FactAttribute
{
    public SandboxMarketplaceFactAttribute()
    {
        if (Sandbox.Current?.SubMerchantId is null)
            Skip = "Sandbox sub-merchant id missing, set Datatrans:SubMerchantId";
    }
}
