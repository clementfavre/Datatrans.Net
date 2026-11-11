using Datatrans.Net.Models;

namespace Datatrans.Net;

/// <summary>Transactions endpoints, <c>/v1/transactions</c></summary>
public interface ITransactionsApi
{
    /// <summary>Initializes a transaction for the Redirect or Lightbox payment page</summary>
    /// <param name="request">Transaction to initialize</param>
    /// <param name="idempotencyKey">Optional <c>Idempotency-Key</c> header, a UUID v4 is recommended</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<InitResponse> InitAsync(InitRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>Authorizes a payment with an alias, without the payment page</summary>
    Task<AuthorizeResponse> AuthorizeAsync(AuthorizeRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>Checks that an alias is still valid, no amount is charged</summary>
    Task<AuthorizeResponse> ValidateAsync(ValidateRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>Authorizes a transaction that was only authenticated, see <c>option.authenticationOnly</c></summary>
    Task<AuthorizeAuthenticatedResponse> AuthorizeAuthenticatedAsync(string transactionId,
        AuthorizeAuthenticatedRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>Increases the amount of an authorized transaction, credit cards only</summary>
    Task<IncreaseResponse> IncreaseAsync(string transactionId, IncreaseRequest request,
        string? idempotencyKey = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the status of a transaction</summary>
    Task<StatusResponse> StatusAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>Settles an authorized transaction</summary>
    Task SettleAsync(string transactionId, SettleRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>Cancels an authorized transaction</summary>
    Task CancelAsync(string transactionId, CancelRequest? request = null, string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>Refunds a settled transaction, a new transaction id is returned</summary>
    Task<CreditResponse> CreditAsync(string transactionId, CreditRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default);
}
