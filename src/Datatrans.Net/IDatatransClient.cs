namespace Datatrans.Net;

/// <summary>Entry point of the Datatrans API</summary>
public interface IDatatransClient
{
    /// <summary>Transactions endpoints</summary>
    ITransactionsApi Transactions { get; }

    /// <summary>Aliases endpoints</summary>
    IAliasesApi Aliases { get; }

    /// <summary>Builds the payment page URL to redirect the customer to</summary>
    Uri GetPaymentPageUrl(string transactionId);
}
