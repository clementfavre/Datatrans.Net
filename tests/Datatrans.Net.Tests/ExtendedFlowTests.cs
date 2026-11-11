using System.Net;
using Datatrans.Net.Models;

namespace Datatrans.Net.Tests;

public class ExtendedFlowTests
{
    [Fact]
    public async Task Authenticated_transaction_is_authorized()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK,
            """{"acquirerAuthorizationCode":"333333"}""");

        var response = await client.Transactions.AuthorizeAuthenticatedAsync("1",
            new AuthorizeAuthenticatedRequest { Refno = "order-45", Amount = 1000, AutoSettle = false });

        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/1/authorize",
            handler.Request!.RequestUri!.ToString());
        Assert.Equal("""{"amount":1000,"refno":"order-45","autoSettle":false}""", handler.RequestBody);
        Assert.Equal("333333", response.AcquirerAuthorizationCode);
    }

    [Fact]
    public async Task Authorized_amount_is_increased()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"increasedAmount":1500}""");

        var response = await client.Transactions.IncreaseAsync("1",
            new IncreaseRequest { Amount = 500, Currency = "CHF", Refno = "order-45" });

        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/1/increase",
            handler.Request!.RequestUri!.ToString());
        Assert.Equal("""{"amount":500,"currency":"CHF","refno":"order-45"}""", handler.RequestBody);
        Assert.Equal(1500, response.IncreasedAmount);
    }

    [Fact]
    public async Task Settle_sends_the_partial_capture()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.NoContent);

        await client.Transactions.SettleAsync("1", new SettleRequest
        {
            Amount = 400,
            Currency = "CHF",
            Refno = "order-45",
            PartialCapture = new PartialCapture { SequenceNumber = 1, TotalNumber = 2 }
        });

        Assert.Equal(
            """{"amount":400,"currency":"CHF","refno":"order-45","partialCapture":{"sequenceNumber":1,"totalNumber":2}}""",
            handler.RequestBody);
    }

    [Fact]
    public async Task Authorize_sends_the_payment_method_aliases()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"transactionId":"6"}""");

        await client.Transactions.AuthorizeAsync(new AuthorizeRequest
        {
            Amount = 100,
            Currency = "CHF",
            Refno = "order-46",
            PayPal = new PayPalAuthorize { Alias = "pp-alias" },
            PostFinanceCard = new PaymentMethodAlias { Alias = "pfc-alias" }
        });

        Assert.Equal(
            """{"amount":100,"currency":"CHF","refno":"order-46","PAP":{"alias":"pp-alias"},"PFC":{"alias":"pfc-alias"}}""",
            handler.RequestBody);
    }

    [Fact]
    public async Task Alias_is_updated()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK,
            """{"alias":"alias-1","card":{"expiryMonth":"07","expiryYear":"30"}}""");

        var info = await client.Aliases.UpdateAsync("alias-1",
            new AliasPatchRequest { ExpiryMonth = "07", ExpiryYear = "30" });

        Assert.Equal(HttpMethod.Patch, handler.Request!.Method);
        Assert.Equal("https://api.sandbox.datatrans.com/v1/aliases/alias-1", handler.Request.RequestUri!.ToString());
        Assert.Equal("""{"expiryMonth":"07","expiryYear":"30"}""", handler.RequestBody);
        Assert.Equal("30", info.Card!.ExpiryYear);
    }
}
