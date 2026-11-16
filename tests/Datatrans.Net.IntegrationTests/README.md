# Integration tests

These tests call the real Datatrans sandbox. They are skipped when the credentials are missing, so `dotnet test` stays green without them.

## Credentials

Locally, with user secrets:

```
cd tests/Datatrans.Net.IntegrationTests
dotnet user-secrets set "Datatrans:MerchantId" "1100000000"
dotnet user-secrets set "Datatrans:Password" "..."
```

Or with environment variables: `Datatrans__MerchantId`, `Datatrans__Password`.

In GitHub Actions, add the repository secrets `DATATRANS_MERCHANT_ID` and `DATATRANS_PASSWORD`.

Only the sandbox is ever called, whatever the credentials.

## Card alias (optional)

Authorize, settle, refund and cancel need a card alias, since a payment through the payment page needs a browser.

To get one, pay a sandbox transaction once with `Option = new InitOptions { CreateAlias = true }` and a test card, then read `Card.Alias` from the status. Then set:

```
dotnet user-secrets set "Datatrans:CardAlias" "..."
dotnet user-secrets set "Datatrans:CardExpiryMonth" "06"
dotnet user-secrets set "Datatrans:CardExpiryYear" "28"
```

The expiry is the one entered on the payment page. In GitHub Actions: `DATATRANS_CARD_ALIAS`, `DATATRANS_CARD_EXPIRY_MONTH`, `DATATRANS_CARD_EXPIRY_YEAR`.

## Run

```
dotnet test tests/Datatrans.Net.IntegrationTests
```
