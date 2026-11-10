using System.Net;
using Datatrans.Net.Models;

namespace Datatrans.Net.Tests;

public class AliasTests
{
    [Fact]
    public async Task Authorize_charges_a_card_alias()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK,
            """{"transactionId":"3","acquirerAuthorizationCode":"222222","card":{"masked":"424242xxxxxx4242"}}""");

        var response = await client.Transactions.AuthorizeAsync(new AuthorizeRequest
        {
            Amount = 1000,
            Currency = "CHF",
            Refno = "order-43",
            AutoSettle = true,
            Card = new AliasCard { Alias = "AAABeCBPbiHssdexyrAAAYkaznYWAPYt", ExpiryMonth = "06", ExpiryYear = "28" }
        }, idempotencyKey: "key-2");

        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/authorize",
            handler.Request.RequestUri!.ToString());
        Assert.Equal("key-2", handler.Request.Headers.GetValues("Idempotency-Key").Single());
        Assert.Equal(
            """{"amount":1000,"currency":"CHF","refno":"order-43","autoSettle":true,"card":{"type":"ALIAS","alias":"AAABeCBPbiHssdexyrAAAYkaznYWAPYt","expiryMonth":"06","expiryYear":"28"}}""",
            handler.RequestBody);

        Assert.Equal("3", response.TransactionId);
        Assert.Equal("222222", response.AcquirerAuthorizationCode);
        Assert.Equal("424242xxxxxx4242", response.Card!.Masked);
    }

    [Fact]
    public async Task Authorize_charges_a_twint_alias()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"transactionId":"4"}""");

        await client.Transactions.AuthorizeAsync(new AuthorizeRequest
        {
            Amount = 500,
            Currency = "CHF",
            Refno = "order-44",
            Twint = new TwintAlias { Alias = "twint-alias" }
        });

        Assert.Equal("""{"amount":500,"currency":"CHF","refno":"order-44","TWI":{"alias":"twint-alias"}}""",
            handler.RequestBody);
    }

    [Fact]
    public async Task Validate_posts_the_alias_without_a_card_type()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, """{"transactionId":"5"}""");

        var response = await client.Transactions.ValidateAsync(new ValidateRequest
        {
            Refno = "check-1",
            Currency = "CHF",
            Card = new ValidateCard { Alias = "alias-1" }
        });

        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/validate",
            handler.Request!.RequestUri!.ToString());
        Assert.Equal("""{"refno":"check-1","currency":"CHF","card":{"alias":"alias-1"}}""", handler.RequestBody);
        Assert.Equal("5", response.TransactionId);
    }

    [Fact]
    public async Task Alias_info_is_read()
    {
        const string json = """
            {
              "alias": "AAABeCBPbiHssdexyrAAAYkaznYWAPYt",
              "fingerprint": "F-coGuJQ55tUk8CnKGI9PX1T",
              "type": "CARD",
              "masked": "490000xxxxxx0003",
              "dateCreated": "2026-11-10T12:33:51Z",
              "card": {
                "expiryMonth": "06",
                "expiryYear": "28",
                "last4": "0003",
                "cardInfo": { "brand": "VISA CREDIT", "type": "credit", "country": "CH" }
              },
              "PFP": { "anything": true }
            }
            """;
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, json);

        var info = await client.Aliases.GetAsync("AAABeCBPbiHssdexyrAAAYkaznYWAPYt");

        Assert.Equal(HttpMethod.Get, handler.Request!.Method);
        Assert.Equal("https://api.sandbox.datatrans.com/v1/aliases/AAABeCBPbiHssdexyrAAAYkaznYWAPYt",
            handler.Request.RequestUri!.ToString());
        Assert.Equal("CARD", info.Type);
        Assert.Equal("490000xxxxxx0003", info.Masked);
        Assert.Equal(new DateTimeOffset(2026, 11, 10, 12, 33, 51, TimeSpan.Zero), info.DateCreated);
        Assert.Equal("0003", info.Card!.Last4);
        Assert.Equal("VISA CREDIT", info.Card.CardInfo!.Brand);
        Assert.True(info.AdditionalProperties!.ContainsKey("PFP"));
    }

    [Fact]
    public async Task Alias_is_deleted()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.NoContent);

        await client.Aliases.DeleteAsync("alias/1");

        Assert.Equal(HttpMethod.Delete, handler.Request!.Method);
        Assert.Equal("https://api.sandbox.datatrans.com/v1/aliases/alias%2F1", handler.Request.RequestUri!.AbsoluteUri);
        Assert.Null(handler.Request.Content);
    }

    [Fact]
    public async Task Unknown_alias_is_mapped_to_an_exception()
    {
        var (client, _) = FakeHttpHandler.CreateClient(HttpStatusCode.NotFound,
            """{"error":{"code":"ALIAS_NOT_FOUND","message":"alias not found"}}""");

        var exception = await Assert.ThrowsAsync<DatatransException>(() => client.Aliases.GetAsync("nope"));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal(DatatransErrorCode.AliasNotFound, exception.ErrorCode);
    }
}
