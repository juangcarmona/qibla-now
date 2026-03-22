using QiblaNow.Core.Models;

namespace QiblaNow.Core.Tests;

public class ReverseGeocodingHelperTests
{
    [Fact]
    public void BuildCacheKey_Rounds_To_Three_Decimals_And_Normalizes_Language()
    {
        var key = ReverseGeocodingHelper.BuildCacheKey(40.416775, -3.703790, " ES ");
        Assert.Equal("40.417|-3.704|es", key);
    }

    [Fact]
    public void SelectDisplayName_Uses_Required_Priority_Order()
    {
        var place = new ResolvedPlace(
            0, 0, "en",
            locality: null,
            sublocality: "Downtown",
            adminArea: "Madrid",
            country: "Spain",
            formattedAddress: "Downtown, Madrid, Spain");

        var label = ReverseGeocodingHelper.SelectDisplayName(place, 40.4, -3.7);
        Assert.Equal("Downtown", label);
    }

    [Fact]
    public void SelectDisplayName_Falls_Back_To_Coordinates_When_No_Place()
    {
        var label = ReverseGeocodingHelper.SelectDisplayName(null, 40.416775, -3.70379);
        Assert.Equal("40.4168, -3.7038", label);
    }
}
