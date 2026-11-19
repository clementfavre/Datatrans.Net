namespace Datatrans.Net.Models;

/// <summary>Split of a transaction between sub-merchants, for marketplace merchants</summary>
public sealed class Marketplace
{
    /// <summary>Shares of the transaction, their amounts add up to the transaction amount</summary>
    public required IList<MarketplaceSplit> Splits { get; set; }
}

/// <summary>Share of a sub-merchant, amounts in the smallest currency unit</summary>
public sealed class MarketplaceSplit
{
    /// <summary>Sub-merchant id, given by the collector</summary>
    public required string SubMerchantId { get; set; }

    /// <summary>Share transferred to or from the sub-merchant, never 0</summary>
    public required long Amount { get; set; }

    /// <summary>Marketplace commission, deducted from the share, at most <see cref="Amount"/></summary>
    public long? Commission { get; set; }

    /// <summary>VAT of the commission, at most <see cref="Commission"/></summary>
    public long? Vat { get; set; }
}
