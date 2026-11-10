using System.Net;
using System.Text;
using System.Text.Json;
using Datatrans.Net.Models;

namespace Datatrans.Net.Tests;

public class TransactionsTests
{
    [Fact]
    public async Task Init_posts_the_request_and_returns_the_payment_page_url()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.Created,
            """{"transactionId":"210707103033915995"}""");

        var response = await client.Transactions.InitAsync(new InitRequest
        {
            Amount = 1000,
            Currency = "CHF",
            Refno = "order-42",
            PaymentMethods = [PaymentMethods.Visa, PaymentMethods.Twint],
            AutoSettle = true,
            Redirect = new RedirectRequest { SuccessUrl = "https://shop.test/ok" },
            AdditionalProperties = new Dictionary<string, object?> { ["theme"] = new { name = "DT2015" } }
        }, idempotencyKey: "key-1");

        Assert.Equal("210707103033915995", response.TransactionId);
        Assert.Equal("https://pay.sandbox.datatrans.com/v1/start/210707103033915995",
            response.PaymentPageUrl!.ToString());

        var request = handler.Request!;
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions", request.RequestUri!.ToString());
        Assert.Equal("Basic", request.Headers.Authorization!.Scheme);
        Assert.Equal("1100007006:secret",
            Encoding.UTF8.GetString(Convert.FromBase64String(request.Headers.Authorization.Parameter!)));
        Assert.Equal("key-1", request.Headers.GetValues("Idempotency-Key").Single());

        using var body = JsonDocument.Parse(handler.RequestBody!);
        var root = body.RootElement;
        Assert.Equal(1000, root.GetProperty("amount").GetInt64());
        Assert.Equal("CHF", root.GetProperty("currency").GetString());
        Assert.Equal("order-42", root.GetProperty("refno").GetString());
        Assert.True(root.GetProperty("autoSettle").GetBoolean());
        Assert.Equal(["VIS", "TWI"], root.GetProperty("paymentMethods").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal("https://shop.test/ok", root.GetProperty("redirect").GetProperty("successUrl").GetString());
        Assert.Equal("DT2015", root.GetProperty("theme").GetProperty("name").GetString());

        // Null properties must not be sent, the API rejects unknown or empty ones
        Assert.False(root.TryGetProperty("refno2", out _));
        Assert.False(root.TryGetProperty("customer", out _));
        Assert.False(root.TryGetProperty("additionalProperties", out _));
    }

    [Fact]
    public async Task Production_environment_uses_the_live_hosts()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.Created,
            """{"transactionId":"1"}""", DatatransEnvironment.Production);

        var response = await client.Transactions.InitAsync(new InitRequest { Currency = "CHF", Refno = "r" });

        Assert.Equal("https://api.datatrans.com/v1/transactions", handler.Request!.RequestUri!.ToString());
        Assert.Equal("https://pay.datatrans.com/v1/start/1", response.PaymentPageUrl!.ToString());
        Assert.False(handler.Request.Headers.Contains("Idempotency-Key"));
    }

    [Fact]
    public async Task Status_reads_the_transaction()
    {
        const string json = """
            {
              "transactionId": "210707103033915995",
              "merchantId": "1100007006",
              "type": "payment",
              "status": "settled",
              "currency": "CHF",
              "refno": "order-42",
              "paymentMethod": "TWI",
              "detail": {
                "init": { "expires": "2026-11-10T12:33:52.141+00:00" },
                "authorize": { "amount": 1000, "acquirerAuthorizationCode": "103033" },
                "settle": { "amount": 1000 }
              },
              "TWI": { "alias": "alias-1", "fingerprint": "F-1" },
              "PAP": { "payerId": "x" },
              "history": [
                { "action": "init", "amount": 1000, "source": "api", "date": "2026-11-10T12:33:51Z", "success": true },
                { "action": "authorize", "autoSettle": true, "amount": 1000, "source": "redirect",
                  "date": "2026-11-10T12:33:52.505+00:00", "success": true }
              ]
            }
            """;
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK, json);

        var status = await client.Transactions.StatusAsync("210707103033915995");

        Assert.Equal(HttpMethod.Get, handler.Request!.Method);
        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/210707103033915995",
            handler.Request.RequestUri!.ToString());
        Assert.Null(handler.Request.Content);

        Assert.Equal(TransactionType.Payment, status.Type);
        Assert.Equal(TransactionStatus.Settled, status.Status);
        Assert.True(status.IsPaid);
        Assert.Equal("TWI", status.PaymentMethod);
        Assert.Equal(1000, status.Detail!.Authorize!.Amount);
        Assert.Equal(1000, status.Detail.Settle!.Amount);
        Assert.Equal(new DateTimeOffset(2026, 11, 10, 12, 33, 52, 141, TimeSpan.Zero), status.Detail.Init!.Expires);
        Assert.Equal("alias-1", status.Twint!.Alias);
        Assert.Equal(2, status.History!.Count);
        Assert.Equal("authorize", status.History[1].Action);
        Assert.True(status.History[1].AutoSettle);
        Assert.True(status.AdditionalProperties!.ContainsKey("PAP"));
    }

    [Fact]
    public async Task Status_tolerates_unknown_enum_values()
    {
        var (client, _) = FakeHttpHandler.CreateClient(HttpStatusCode.OK,
            """{"transactionId":"1","type":"brand_new","status":"brand_new","detail":{"fail":{"reason":"soft_declined"}}}""");

        var status = await client.Transactions.StatusAsync("1");

        Assert.Equal(TransactionType.Unknown, status.Type);
        Assert.Equal(TransactionStatus.Unknown, status.Status);
        Assert.False(status.IsPaid);
        Assert.Equal(FailReason.SoftDeclined, status.Detail!.Fail!.Reason);
    }

    [Fact]
    public async Task Settle_posts_to_the_settle_endpoint()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.NoContent);

        await client.Transactions.SettleAsync("abc/1", new SettleRequest
        {
            Amount = 500,
            Currency = "CHF",
            Refno = "order-42"
        });

        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/abc%2F1/settle",
            handler.Request!.RequestUri!.AbsoluteUri);
        Assert.Equal("""{"amount":500,"currency":"CHF","refno":"order-42"}""", handler.RequestBody);
    }

    [Fact]
    public async Task Cancel_posts_an_empty_object_by_default()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.NoContent);

        await client.Transactions.CancelAsync("1");

        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/1/cancel",
            handler.Request!.RequestUri!.ToString());
        Assert.Equal("{}", handler.RequestBody);
    }

    [Fact]
    public async Task Credit_returns_the_new_transaction()
    {
        var (client, handler) = FakeHttpHandler.CreateClient(HttpStatusCode.OK,
            """{"transactionId":"2","acquirerAuthorizationCode":"111111"}""");

        var response = await client.Transactions.CreditAsync("1", new CreditRequest
        {
            Amount = 250,
            Currency = "CHF",
            Refno = "refund-1"
        });

        Assert.Equal("https://api.sandbox.datatrans.com/v1/transactions/1/credit",
            handler.Request!.RequestUri!.ToString());
        Assert.Equal("2", response.TransactionId);
        Assert.Equal("111111", response.AcquirerAuthorizationCode);
    }

    [Fact]
    public async Task Api_error_is_mapped_to_an_exception()
    {
        var (client, _) = FakeHttpHandler.CreateClient(HttpStatusCode.BadRequest,
            """{"error":{"code":"INVALID_TRANSACTION_STATUS","message":"cannot settle"}}""");

        var exception = await Assert.ThrowsAsync<DatatransException>(() =>
            client.Transactions.SettleAsync("1", new SettleRequest { Amount = 1, Currency = "CHF", Refno = "r" }));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal(DatatransErrorCode.InvalidTransactionStatus, exception.ErrorCode);
        Assert.Equal("INVALID_TRANSACTION_STATUS", exception.RawErrorCode);
        Assert.Equal("cannot settle", exception.Message);
    }

    [Fact]
    public async Task Non_json_error_keeps_the_raw_body()
    {
        var (client, _) = FakeHttpHandler.CreateClient(HttpStatusCode.BadGateway, "<html>Bad gateway</html>");

        var exception = await Assert.ThrowsAsync<DatatransException>(() => client.Transactions.StatusAsync("1"));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal(DatatransErrorCode.Unknown, exception.ErrorCode);
        Assert.Null(exception.RawErrorCode);
        Assert.Equal("<html>Bad gateway</html>", exception.ResponseBody);
    }

    [Fact]
    public void Missing_credentials_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => new DatatransClient(new DatatransOptions { Password = "x" }));
        Assert.Throws<ArgumentException>(() => new DatatransClient(new DatatransOptions { MerchantId = "x" }));
    }
}
