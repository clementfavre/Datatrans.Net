using System.Security.Cryptography;
using System.Text;
using Datatrans.Net.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Datatrans.Net.Tests;

public class WebhookTests
{
    // Test vector published in the Datatrans documentation, "Signing your webhooks"
    private const string Key =
        "861bbfc01e089259091927d6ad7f71c8b46b7ee13499574e83c633b74cdc29e3b7e262e41318c8425c520f146986675fdd58a4531a01c99f06da378fdab0414a";

    private const string Header =
        "t=1605697463367,s0=82ef9a8178dcb4df0b71540fa06d7da826ecb26e1977e230bdc8c9d6f9f1af84";

    [Fact]
    public void Documented_signature_example_is_valid()
    {
        Assert.True(DatatransWebhook.IsSignatureValid(Header, "HELLO", Key));
    }

    [Theory]
    [InlineData("HELLO ")]
    [InlineData("hello")]
    public void Altered_payload_is_rejected(string payload)
    {
        Assert.False(DatatransWebhook.IsSignatureValid(Header, payload, Key));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("s0=82ef9a8178dcb4df0b71540fa06d7da826ecb26e1977e230bdc8c9d6f9f1af84")]
    [InlineData("t=1605697463367")]
    [InlineData("t=1605697463367,s0=not-hex")]
    [InlineData("t=1605697463368,s0=82ef9a8178dcb4df0b71540fa06d7da826ecb26e1977e230bdc8c9d6f9f1af84")]
    public void Malformed_or_wrong_header_is_rejected(string? header)
    {
        Assert.False(DatatransWebhook.IsSignatureValid(header, "HELLO", Key));
    }

    [Fact]
    public void Old_timestamp_is_rejected_when_a_tolerance_is_set()
    {
        Assert.False(DatatransWebhook.IsSignatureValid(Header, "HELLO", Key, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Recent_timestamp_is_accepted_when_a_tolerance_is_set()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var signature = Convert.ToHexString(HMACSHA256.HashData(Convert.FromHexString(Key),
            Encoding.UTF8.GetBytes(timestamp + "{}"))).ToLowerInvariant();

        Assert.True(DatatransWebhook.IsSignatureValid($"t={timestamp},s0={signature}", "{}", Key,
            TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Payload_is_parsed_as_a_status()
    {
        var payload = DatatransWebhook.Parse(
            """{"transactionId":"1","status":"authorized","refno":"order-42","card":{"masked":"424242xxxxxx4242"}}""");

        Assert.Equal(TransactionStatus.Authorized, payload.Status);
        Assert.Equal("order-42", payload.Refno);
        Assert.Equal("424242xxxxxx4242", payload.Card!.Masked);
    }

    [Fact]
    public void AddDatatrans_registers_the_client()
    {
        var services = new ServiceCollection();
        services.AddDatatrans(options =>
        {
            options.MerchantId = "1100007006";
            options.Password = "secret";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IDatatransClient>();

        Assert.Equal("https://pay.sandbox.datatrans.com/v1/start/1", client.GetPaymentPageUrl("1").ToString());
    }
}
