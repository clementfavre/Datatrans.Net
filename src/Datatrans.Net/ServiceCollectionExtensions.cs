using Datatrans.Net;
using Microsoft.Extensions.Options;

// Same namespace as IServiceCollection so the extension shows up without an extra using
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registration of the Datatrans client</summary>
public static class DatatransServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IDatatransClient"/> as a typed HTTP client</summary>
    public static IHttpClientBuilder AddDatatrans(this IServiceCollection services,
        Action<DatatransOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<DatatransOptions>().Configure(configure);

        return services.AddHttpClient<IDatatransClient, DatatransClient>((httpClient, provider) =>
            new DatatransClient(httpClient, provider.GetRequiredService<IOptions<DatatransOptions>>().Value));
    }
}
