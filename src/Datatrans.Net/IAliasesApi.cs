using Datatrans.Net.Models;

namespace Datatrans.Net;

/// <summary>Aliases endpoints, <c>/v1/aliases</c></summary>
public interface IAliasesApi
{
    /// <summary>Gets the details of an alias</summary>
    Task<AliasInfoResponse> GetAsync(string alias, CancellationToken cancellationToken = default);

    /// <summary>Updates an alias, for example its expiry date</summary>
    Task<AliasInfoResponse> UpdateAsync(string alias, AliasPatchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an alias, it can no longer be charged afterwards</summary>
    Task DeleteAsync(string alias, CancellationToken cancellationToken = default);
}
