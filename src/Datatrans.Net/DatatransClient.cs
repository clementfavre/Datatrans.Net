using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Datatrans.Net.Models;
using Datatrans.Net.Serialization;

namespace Datatrans.Net;

/// <summary>Client of the Datatrans API</summary>
public sealed class DatatransClient : IDatatransClient, ITransactionsApi, IAliasesApi
{
    private const string IdempotencyHeader = "Idempotency-Key";
    private const string TransactionsPath = "/v1/transactions";
    private const string AliasesPath = "/v1/aliases";

    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _paymentPageBaseUrl;
    private readonly AuthenticationHeaderValue _authorization;

    /// <summary>Creates a client with its own <see cref="HttpClient"/></summary>
    public DatatransClient(DatatransOptions options) : this(new HttpClient(), options)
    {
    }

    /// <summary>Creates a client on top of an existing <see cref="HttpClient"/></summary>
    public DatatransClient(HttpClient httpClient, DatatransOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        _httpClient = httpClient;
        _apiBaseUrl = options.ResolveApiBaseUrl();
        _paymentPageBaseUrl = options.ResolvePaymentPageBaseUrl();
        _authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.MerchantId}:{options.Password}")));
    }

    /// <inheritdoc />
    public ITransactionsApi Transactions => this;

    /// <inheritdoc />
    public IAliasesApi Aliases => this;

    /// <inheritdoc />
    public Uri GetPaymentPageUrl(string transactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);

        return new Uri($"{_paymentPageBaseUrl}/v1/start/{Uri.EscapeDataString(transactionId)}");
    }

    /// <inheritdoc />
    public async Task<InitResponse> InitAsync(InitRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await SendAsync<InitResponse>(HttpMethod.Post, TransactionsPath, request, idempotencyKey,
            cancellationToken).ConfigureAwait(false);
        response.PaymentPageUrl = GetPaymentPageUrl(response.TransactionId);

        return response;
    }

    /// <inheritdoc />
    public Task<AuthorizeResponse> AuthorizeAsync(AuthorizeRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return SendAsync<AuthorizeResponse>(HttpMethod.Post, TransactionsPath + "/authorize", request,
            idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AuthorizeResponse> ValidateAsync(ValidateRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return SendAsync<AuthorizeResponse>(HttpMethod.Post, TransactionsPath + "/validate", request,
            idempotencyKey, cancellationToken);
    }

    Task<AliasInfoResponse> IAliasesApi.GetAsync(string alias, CancellationToken cancellationToken) =>
        SendAsync<AliasInfoResponse>(HttpMethod.Get, AliasPath(alias), null, null, cancellationToken);

    Task IAliasesApi.DeleteAsync(string alias, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Delete, AliasPath(alias), null, null, cancellationToken);

    /// <inheritdoc />
    public Task<StatusResponse> StatusAsync(string transactionId, CancellationToken cancellationToken = default) =>
        SendAsync<StatusResponse>(HttpMethod.Get, TransactionPath(transactionId), null, null, cancellationToken);

    /// <inheritdoc />
    public Task SettleAsync(string transactionId, SettleRequest request, string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return SendAsync(HttpMethod.Post, TransactionPath(transactionId) + "/settle", request, idempotencyKey,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task CancelAsync(string transactionId, CancelRequest? request = null, string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, TransactionPath(transactionId) + "/cancel", request ?? new CancelRequest(),
            idempotencyKey, cancellationToken);

    /// <inheritdoc />
    public Task<CreditResponse> CreditAsync(string transactionId, CreditRequest request,
        string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return SendAsync<CreditResponse>(HttpMethod.Post, TransactionPath(transactionId) + "/credit", request,
            idempotencyKey, cancellationToken);
    }

    private static string AliasPath(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        return $"{AliasesPath}/{Uri.EscapeDataString(alias)}";
    }

    private static string TransactionPath(string transactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);

        return $"{TransactionsPath}/{Uri.EscapeDataString(transactionId)}";
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(method, path, body, idempotencyKey, cancellationToken)
            .ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return JsonSerializer.Deserialize<T>(content, DatatransJson.Options)
               ?? throw new JsonException($"Empty response body for {method} {path}");
    }

    private async Task SendAsync(HttpMethod method, string path, object? body, string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(method, path, body, idempotencyKey, cancellationToken)
            .ConfigureAwait(false);
    }

    // Sends the request and throws a DatatransException on a non success status code
    private async Task<HttpResponseMessage> SendCoreAsync(HttpMethod method, string path, object? body,
        string? idempotencyKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, _apiBaseUrl + path);
        request.Headers.Authorization = _authorization;
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            request.Headers.Add(IdempotencyHeader, idempotencyKey);

        if (body != null)
        {
            // Serialized upfront so the request carries a Content-Length instead of being chunked
            var json = JsonSerializer.Serialize(body, body.GetType(), DatatransJson.Options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
            return response;

        using (response)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw CreateException(response, content);
        }
    }

    private static DatatransException CreateException(HttpResponseMessage response, string content)
    {
        ErrorBody? error = null;
        try
        {
            error = JsonSerializer.Deserialize<ErrorEnvelope>(content, DatatransJson.Options)?.Error;
        }
        catch (JsonException)
        {
            // Not a JSON error body, the raw content is kept on the exception
        }

        var code = DatatransErrorCode.Unknown;
        if (error?.Code != null)
            Enum.TryParse(error.Code.Replace("_", string.Empty), ignoreCase: true, out code);

        var message = error?.Message ?? $"Datatrans API call failed with status {(int)response.StatusCode}";

        return new DatatransException(response.StatusCode, code, error?.Code, message, content);
    }

    private sealed class ErrorEnvelope
    {
        public ErrorBody? Error { get; set; }
    }

    private sealed class ErrorBody
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
