using System.Text.Json.Serialization;

namespace Datatrans.Net.Models;

/// <summary>Order sent with a transaction, amounts in the smallest currency unit</summary>
public sealed class Order
{
#pragma warning disable CS1591
    public IList<Article>? Articles { get; set; }
    public long? TaxAmount { get; set; }
    public long? ShippingAmount { get; set; }
    public long? DiscountAmount { get; set; }
#pragma warning restore CS1591
}

/// <summary>Line of an order, amounts in the smallest currency unit</summary>
public sealed class Article
{
#pragma warning disable CS1591
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    /// <summary>HTTPS URL of the article image</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Line price including VAT</summary>
    public long? Price { get; set; }

    /// <summary>Article gross price including VAT</summary>
    public long? PriceGross { get; set; }

    public int? Quantity { get; set; }

    /// <summary>Tax rate in percent</summary>
    public double? TaxPercent { get; set; }

    public long? TaxAmount { get; set; }

    [JsonPropertyName("priceWithoutVAT")]
    public long? PriceWithoutVat { get; set; }

    public long? Discount { get; set; }

    /// <summary>Article type, possible values depend on the payment method</summary>
    public string? Type { get; set; }
#pragma warning restore CS1591
}

/// <summary>Payment page theme</summary>
public sealed class Theme
{
    /// <summary>Options of the default theme</summary>
    public ThemeConfiguration? Configuration { get; set; }
}

/// <summary>Options of the default payment page theme</summary>
public sealed class ThemeConfiguration
{
    /// <summary>Brand color in hex notation, for example <c>#336699</c></summary>
    public string? BrandColor { get; set; }

    /// <summary>Header text color when no logo is set, <c>white</c> or <c>black</c></summary>
    public string? TextColor { get; set; }

    /// <summary>Header logo style, <c>circle</c>, <c>rectangle</c> or <c>none</c></summary>
    public string? LogoType { get; set; }

    /// <summary>Logo border, <c>true</c> for the default color or a color value</summary>
    public string? LogoBorderColor { get; set; }

    /// <summary>Whether the pay button uses the brand color, <c>true</c> or <c>false</c></summary>
    public string? BrandButton { get; set; }

    /// <summary>Pay button text color, <c>black</c> or <c>white</c></summary>
    public string? PayButtonTextColor { get; set; }

    /// <summary>SVG logo, uploaded beforehand in the Datatrans web administration</summary>
    public string? LogoSrc { get; set; }

    /// <summary>Payment method selection layout, <c>list</c> or <c>grid</c></summary>
    public string? InitialView { get; set; }

    /// <summary>When false and no logo is set, the header stays empty</summary>
    public bool? BrandTitle { get; set; }
}
