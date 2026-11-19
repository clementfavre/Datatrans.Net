using System.Net;
using System.Text.Json;
using Datatrans.Net.Models;

namespace Datatrans.Net.Tests;

public class MarketplaceTests
{
    // One split with every field, one without the optional ones
    private static Marketplace CreateMarketplace() => new()
    {
        Splits =
        [
            new MarketplaceSplit { SubMerchantId = "sub-1", Amount = 700, Commission = 70, Vat = 5 },
            new MarketplaceSplit { SubMerchantId = "sub-2", Amount = 300 }
        ]
    };

    private static void AssertSplits(string? requestBody)
    {
        using var body = JsonDocument.Parse(requestBody!);
        var splits = body.RootElement.GetProperty("marketplace").GetProperty("splits");
        Assert.Equal(2, splits.GetArrayLength());

        var first = splits[0];
        Assert.Equal("sub-1", first.GetProperty("subMerchantId").GetString());
        Assert.Equal(700, first.GetProperty("amount").GetInt64());
        Assert.Equal(70, first.GetProperty("commission").GetInt64());
        Assert.Equal(5, first.GetProperty("vat").GetInt64());

        var second = splits[1];
        Assert.Equal("sub-2", second.GetProperty("subMerchantId").GetString());
        Assert.Equal(300, second.GetProperty("amount").GetInt64());
        Assert.False(second.TryGetProperty("commission", out _));
        Assert.False(second.TryGetProperty("vat", out _));
    }

    [Fact]
    public async Task Init_sends_the_marketplace_splits()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.Created, """{"transactionId":"1"}""");

        await client.Transactions.InitAsync(new InitRequest
        {
            Amount = 1000,
            Currency = "CHF",
            Refno = "order-50",
            AutoSettle = true,
            Marketplace = CreateMarketplace()
        });

        AssertSplits(handler.RequestBody);
    }

    [Fact]
    public async Task Authorize_sends_the_marketplace_splits()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"transactionId":"2"}""");

        await client.Transactions.AuthorizeAsync(new AuthorizeRequest
        {
            Amount = 1000,
            Currency = "CHF",
            Refno = "order-51",
            Card = new AliasCard { Alias = "alias-1" },
            Marketplace = CreateMarketplace()
        });

        AssertSplits(handler.RequestBody);
    }

    [Fact]
    public async Task Settle_sends_the_marketplace_splits()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.NoContent);

        await client.Transactions.SettleAsync("1", new SettleRequest
        {
            Amount = 1000,
            Currency = "CHF",
            Refno = "order-50",
            Marketplace = CreateMarketplace()
        });

        AssertSplits(handler.RequestBody);
    }

    [Fact]
    public async Task Credit_sends_the_marketplace_splits()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"transactionId":"3"}""");

        await client.Transactions.CreditAsync("1", new CreditRequest
        {
            Amount = 1000,
            Currency = "CHF",
            Refno = "refund-50",
            Marketplace = CreateMarketplace()
        });

        AssertSplits(handler.RequestBody);
    }

    [Fact]
    public async Task Requests_without_marketplace_do_not_send_the_key()
    {
        var (init, initHandler) = FakeHttpHandler.CreateClient(HttpStatusCode.Created, """{"transactionId":"1"}""");
        await init.Transactions.InitAsync(new InitRequest { Amount = 1000, Currency = "CHF", Refno = "r" });

        var (authorize, authorizeHandler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"transactionId":"2"}""");
        await authorize.Transactions.AuthorizeAsync(new AuthorizeRequest { Amount = 1000, Currency = "CHF", Refno = "r" });

        var (settle, settleHandler) = FakeHttpHandler.CreateClient(HttpStatusCode.NoContent);
        await settle.Transactions.SettleAsync("1", new SettleRequest { Amount = 1000, Currency = "CHF", Refno = "r" });

        var (credit, creditHandler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"transactionId":"3"}""");
        await credit.Transactions.CreditAsync("1", new CreditRequest { Amount = 1000, Currency = "CHF", Refno = "r" });

        foreach (var handler in new[] { initHandler, authorizeHandler, settleHandler, creditHandler })
        {
            using var body = JsonDocument.Parse(handler.RequestBody!);
            Assert.False(body.RootElement.TryGetProperty("marketplace", out _));
        }
    }

    [Fact]
    public async Task Status_reads_the_marketplace_of_the_history()
    {
        const string json = """
            {
              "transactionId": "1",
              "status": "settled",
              "history": [
                { "action": "init", "amount": 1000, "success": true,
                  "marketplace": { "splits": [
                    { "subMerchantId": "sub-1", "amount": 700, "commission": 70, "vat": 5 },
                    { "subMerchantId": "sub-2", "amount": 300 }
                  ] } },
                { "action": "authorize", "autoSettle": true, "amount": 1000, "success": true }
              ]
            }
            """;
        var (client, _) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, json);

        var status = await client.Transactions.StatusAsync("1");

        var splits = status.History![0].Marketplace!.Splits;
        Assert.Equal(2, splits.Count);
        Assert.Equal("sub-1", splits[0].SubMerchantId);
        Assert.Equal(700, splits[0].Amount);
        Assert.Equal(70, splits[0].Commission);
        Assert.Equal(5, splits[0].Vat);
        Assert.Equal("sub-2", splits[1].SubMerchantId);
        Assert.Null(splits[1].Commission);
        Assert.Null(splits[1].Vat);
        Assert.Null(status.History[1].Marketplace);
    }

    [Fact]
    public void Webhook_payload_reads_the_marketplace_of_the_history()
    {
        var payload = DatatransWebhook.Parse(
            """{"transactionId":"1","status":"authorized","history":[{"action":"authorize","marketplace":{"splits":[{"subMerchantId":"sub-1","amount":1000}]}}]}""");

        var split = Assert.Single(payload.History![0].Marketplace!.Splits);
        Assert.Equal("sub-1", split.SubMerchantId);
        Assert.Equal(1000, split.Amount);
    }
}
