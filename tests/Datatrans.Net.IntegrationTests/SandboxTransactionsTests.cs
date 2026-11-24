using System.Net;
using Datatrans.Net.Models;

namespace Datatrans.Net.IntegrationTests;

/// <summary>Calls the real Datatrans sandbox, skipped without credentials</summary>
[Trait("Category", "Integration")]
public class SandboxTransactionsTests
{
    private const string Currency = "CHF";

    private static Sandbox Settings => Sandbox.Current!;

    // A marketplace merchant must split every settle and refund, null on a regular merchant
    private static Marketplace? Split(long amount) => Settings.SubMerchantId is null
        ? null
        : new Marketplace { Splits = [new MarketplaceSplit { SubMerchantId = Settings.SubMerchantId, Amount = amount }] };

    // One method per case, so a method not enabled on the merchant shows up on its own
    [SandboxTheory]
    [InlineData(PaymentMethods.Visa)]
    [InlineData(PaymentMethods.Mastercard)]
    [InlineData(PaymentMethods.AmericanExpress)]
    [InlineData(PaymentMethods.Twint)]
    [InlineData(PaymentMethods.ApplePay)]
    [InlineData(PaymentMethods.GooglePay)]
    public async Task Init_returns_a_payment_page_for_the_payment_method(string paymentMethod)
    {
        var client = Settings.CreateClient();
        var refno = Sandbox.NewRefno();

        var init = await client.Transactions.InitAsync(new InitRequest
        {
            Amount = 1000,
            Currency = Currency,
            Refno = refno,
            PaymentMethods = [paymentMethod],
            Redirect = Sandbox.Redirect
        });

        Assert.False(string.IsNullOrWhiteSpace(init.TransactionId));
        Assert.StartsWith("https://pay.sandbox.datatrans.com/v1/start/", init.PaymentPageUrl!.ToString());

        var status = await client.Transactions.StatusAsync(init.TransactionId);
        Assert.Equal(TransactionType.Payment, status.Type);
        Assert.Equal(TransactionStatus.Initialized, status.Status);
        Assert.Equal(refno, status.Refno);
        Assert.Equal(Currency, status.Currency);
        Assert.False(status.IsPaid);
    }

    // Only proves the format, the sub-merchant id is not checked at init
    [SandboxMarketplaceFact]
    public async Task Init_with_auto_settle_accepts_a_marketplace_split()
    {
        var client = Settings.CreateClient();

        var init = await client.Transactions.InitAsync(new InitRequest
        {
            Amount = 1000,
            Currency = Currency,
            Refno = Sandbox.NewRefno(),
            AutoSettle = true,
            Marketplace = new Marketplace
            {
                Splits = [new MarketplaceSplit { SubMerchantId = Settings.SubMerchantId!, Amount = 1000 }]
            },
            Redirect = Sandbox.Redirect
        });

        Assert.False(string.IsNullOrWhiteSpace(init.TransactionId));

        var status = await client.Transactions.StatusAsync(init.TransactionId);
        Assert.Equal(TransactionStatus.Initialized, status.Status);
    }

    [SandboxFact]
    public async Task Status_of_an_unknown_transaction_throws()
    {
        var client = Settings.CreateClient();

        var exception = await Assert.ThrowsAsync<DatatransException>(() =>
            client.Transactions.StatusAsync("000000000000000000"));

        Assert.InRange((int)exception.StatusCode, 400, 499);
    }

    [SandboxFact]
    public async Task Wrong_password_is_rejected()
    {
        var client = Settings.CreateClient(password: "wrong-password");

        var exception = await Assert.ThrowsAsync<DatatransException>(() =>
            client.Transactions.InitAsync(new InitRequest
            {
                Amount = 1000,
                Currency = Currency,
                Refno = Sandbox.NewRefno(),
                Redirect = Sandbox.Redirect
            }));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [SandboxAliasFact]
    public async Task Alias_can_be_read()
    {
        var client = Settings.CreateClient();

        var alias = await client.Aliases.GetAsync(Settings.CardAlias!);

        Assert.Equal(Settings.CardAlias, alias.Alias);
    }

    [SandboxAliasFact]
    public async Task Alias_payment_can_be_authorized_settled_and_refunded()
    {
        var client = Settings.CreateClient();
        var refno = Sandbox.NewRefno();

        var authorize = await client.Transactions.AuthorizeAsync(new AuthorizeRequest
        {
            Amount = 1000,
            Currency = Currency,
            Refno = refno,
            AutoSettle = false,
            Card = Settings.CreateAliasCard()
        });

        var authorized = await client.Transactions.StatusAsync(authorize.TransactionId);
        Assert.Equal(TransactionStatus.Authorized, authorized.Status);
        Assert.True(authorized.IsPaid);

        await client.Transactions.SettleAsync(authorize.TransactionId,
            new SettleRequest { Amount = 1000, Currency = Currency, Refno = refno, Marketplace = Split(1000) });

        var settled = await client.Transactions.StatusAsync(authorize.TransactionId);
        Assert.Equal(TransactionStatus.Settled, settled.Status);

        var credit = await client.Transactions.CreditAsync(authorize.TransactionId,
            new CreditRequest { Amount = 500, Currency = Currency, Refno = Sandbox.NewRefno(), Marketplace = Split(500) });

        var refund = await client.Transactions.StatusAsync(credit.TransactionId);
        Assert.Equal(TransactionType.Credit, refund.Type);
    }

    [SandboxAliasFact]
    public async Task Alias_payment_can_be_canceled()
    {
        var client = Settings.CreateClient();

        var authorize = await client.Transactions.AuthorizeAsync(new AuthorizeRequest
        {
            Amount = 1000,
            Currency = Currency,
            Refno = Sandbox.NewRefno(),
            AutoSettle = false,
            Card = Settings.CreateAliasCard()
        });

        await client.Transactions.CancelAsync(authorize.TransactionId);

        var status = await client.Transactions.StatusAsync(authorize.TransactionId);
        Assert.Equal(TransactionStatus.Canceled, status.Status);
    }
}
