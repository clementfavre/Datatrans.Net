# Datatrans.Net

Unofficial .NET client for the [Datatrans](https://www.datatrans.ch) payment API (`net8.0`, `net10.0`).

Not affiliated with Datatrans AG.

## Install

```
dotnet add package Datatrans.Net
```

## Usage

```csharp
builder.Services.AddDatatrans(options =>
{
    options.MerchantId = "...";
    options.Password = "...";
    options.Environment = DatatransEnvironment.Sandbox;
});
```

```csharp
// Initialize and redirect the customer
var init = await client.Transactions.InitAsync(new InitRequest
{
    Amount = 1000, // CHF 10.00
    Currency = "CHF",
    Refno = "order-42"
});
return Redirect(init.PaymentPageUrl!.ToString());

// Check the result
var status = await client.Transactions.StatusAsync(transactionId);
if (status.IsPaid) { }

// Settle, cancel, refund
await client.Transactions.SettleAsync(transactionId, new SettleRequest { Amount = 1000, Currency = "CHF", Refno = "order-42" });
await client.Transactions.CancelAsync(transactionId);
await client.Transactions.CreditAsync(transactionId, new CreditRequest { Amount = 500, Currency = "CHF", Refno = "refund-42" });
```

Webhook, on the raw request body:

```csharp
if (!DatatransWebhook.IsSignatureValid(signatureHeader, rawBody, hmacKey))
    return Results.Unauthorized();

var payload = DatatransWebhook.Parse(rawBody);
```

API errors throw a `DatatransException`. See the official [API reference](https://api-reference.datatrans.ch/) for the fields.

## License

[MIT](LICENSE)
