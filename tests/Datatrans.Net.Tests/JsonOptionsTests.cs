using System.Text.Json;
using System.Text.Json.Serialization;
using Datatrans.Net.Serialization;

namespace Datatrans.Net.Tests;

public class JsonOptionsTests
{
    [Fact]
    public void Shared_options_cannot_be_modified()
    {
        Assert.True(DatatransJson.Options.IsReadOnly);
        Assert.Throws<InvalidOperationException>(() =>
            DatatransJson.Options.DefaultIgnoreCondition = JsonIgnoreCondition.Never);
    }

    [Fact]
    public void A_copy_of_the_shared_options_can_be_customized()
    {
        var options = new JsonSerializerOptions(DatatransJson.Options) { WriteIndented = true };

        Assert.False(options.IsReadOnly);
        Assert.True(options.WriteIndented);
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
        Assert.Equal(DatatransJson.Options.Converters.Count, options.Converters.Count);
    }
}
