using System.Net;
using System.Text;

namespace Datatrans.Net.Tests;

/// <summary>Records the outgoing request and answers with a canned response</summary>
internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string? _responseBody;

    public FakeHttpHandler(HttpStatusCode statusCode, string? responseBody = null)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
    }

    public HttpRequestMessage? Request { get; private set; }

    public string? RequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Request = request;
        if (request.Content != null)
            RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        var response = new HttpResponseMessage(_statusCode);
        if (_responseBody != null)
            response.Content = new StringContent(_responseBody, Encoding.UTF8, "application/json");

        return response;
    }

    public static (DatatransClient Client, FakeHttpHandler Handler) CreateClient(HttpStatusCode statusCode,
        string? responseBody = null, DatatransEnvironment environment = DatatransEnvironment.Sandbox)
    {
        var handler = new FakeHttpHandler(statusCode, responseBody);
        var client = new DatatransClient(new HttpClient(handler), new DatatransOptions
        {
            MerchantId = "1100007006",
            Password = "secret",
            Environment = environment
        });

        return (client, handler);
    }
}
