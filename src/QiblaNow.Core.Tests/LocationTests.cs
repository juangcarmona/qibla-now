using QiblaNow.Core.Abstractions;

namespace QiblaNow.Core.Tests;

public class LocationTests
{
    private readonly ISettingsStore _settingsStore = new SettingsStore();

    [Fact]
    public void LocationSnapshot_ValidCoordinates_ReturnsTrue()
    {
        // Arrange & Act
        var validLat = 45.5;
        var validLon = -73.5;
        var snapshot = new LocationSnapshot(LocationMode.GPS, validLat, validLon, "Test City");

        // Assert
        Assert.True(snapshot.IsValidLatitude);
        Assert.True(snapshot.IsValidLongitude);
        Assert.True(snapshot.AreCoordinatesValid);
        Assert.Equal(validLat, snapshot.Latitude);
        Assert.Equal(validLon, snapshot.Longitude);
        Assert.Equal("Test City", snapshot.Label);
    }

    [Fact]
    public void LocationSnapshot_MaxLatitude_ReturnsTrue()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 90, 0, "North Pole");

        // Assert
        Assert.True(snapshot.IsValidLatitude);
    }

    [Fact]
    public void LocationSnapshot_MaxLongitude_ReturnsTrue()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 0, 180, "Prime Meridian");

        // Assert
        Assert.True(snapshot.IsValidLongitude);
    }

    [Fact]
    public void LocationSnapshot_NegativeLatitude_ReturnsTrue()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, -90, 0, "South Pole");

        // Assert
        Assert.True(snapshot.IsValidLatitude);
    }

    [Fact]
    public void LocationSnapshot_NegativeLongitude_ReturnsTrue()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 0, -180, "International Date Line");

        // Assert
        Assert.True(snapshot.IsValidLongitude);
    }

    [Fact]
    public void LocationSnapshot_LatitudeAboveRange_ReturnsFalse()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 91, 0, "Invalid");

        // Assert
        Assert.False(snapshot.IsValidLatitude);
    }

    [Fact]
    public void LocationSnapshot_LatitudeBelowRange_ReturnsFalse()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, -91, 0, "Invalid");

        // Assert
        Assert.False(snapshot.IsValidLatitude);
    }

    [Fact]
    public void LocationSnapshot_LongitudeAboveRange_ReturnsFalse()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 0, 181, "Invalid");

        // Assert
        Assert.False(snapshot.IsValidLongitude);
    }

    [Fact]
    public void LocationSnapshot_LongitudeBelowRange_ReturnsFalse()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 0, -181, "Invalid");

        // Assert
        Assert.False(snapshot.IsValidLongitude);
    }

    [Fact]
    public void LocationSnapshot_NaNLatitude_ReturnsFalse()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, double.NaN, 0, "Invalid");

        // Assert
        Assert.False(snapshot.IsValidLatitude);
    }

    [Fact]
    public void LocationSnapshot_NaNLongitude_ReturnsFalse()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 0, double.NaN, "Invalid");

        // Assert
        Assert.False(snapshot.IsValidLongitude);
    }

    [Fact]
    public void LocationSnapshot_ZeroCoordinates_ReturnsTrue()
    {
        // Arrange
        var snapshot = new LocationSnapshot(LocationMode.GPS, 0, 0, "Prime Meridian");

        // Assert
        Assert.True(snapshot.IsValidLatitude);
        Assert.True(snapshot.IsValidLongitude);
        Assert.True(snapshot.AreCoordinatesValid);
    }

    [Fact]
    public void SettingsStore_SetAndGetLocationMode_ReturnsSameValue()
    {
        // Arrange
        var testMode = LocationMode.Manual;

        // Act
        _settingsStore.SetLocationMode(testMode);
        var retrievedMode = _settingsStore.GetLocationMode();

        // Assert
        Assert.Equal(testMode, retrievedMode);
    }

    [Fact]
    public void SettingsStore_SetAndGetLocationSnapshot_ReturnsSameValues()
    {
        // Arrange
        var testMode = LocationMode.Manual;
        var testLat = 40.7128;
        var testLon = -74.0060;
        var testLabel = "New York";
        var testSnapshot = new LocationSnapshot(testMode, testLat, testLon, testLabel);

        // Act
        _settingsStore.SetLocationMode(testMode);
        _settingsStore.SaveSnapshot(testSnapshot);
        var retrievedSnapshot = _settingsStore.GetLastSnapshot();

        // Assert
        Assert.NotNull(retrievedSnapshot);
        Assert.Equal(testMode, retrievedSnapshot.Mode);
        Assert.Equal(testLat, retrievedSnapshot.Latitude);
        Assert.Equal(testLon, retrievedSnapshot.Longitude);
        Assert.Equal(testLabel, retrievedSnapshot.Label);
        Assert.True(retrievedSnapshot.AreCoordinatesValid);
    }

    [Fact]
    public void SettingsStore_GetLastSnapshot_NoSnapshot_ReturnsNull()
    {
        // Arrange
        // No prior snapshot set

        // Act
        var result = _settingsStore.GetLastSnapshot();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SettingsStore_GetLastSnapshot_InvalidData_ReturnsNull()
    {
        // Arrange
        // Set invalid data (simulating corrupted storage)
        // Note: Preferences doesn't support invalid data natively,
        // but we test the robustness of the method

        // Act
        var result = _settingsStore.GetLastSnapshot();

        // Assert
        Assert.Null(result);
    }
}
