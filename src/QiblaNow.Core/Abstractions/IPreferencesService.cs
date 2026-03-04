namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Core abstraction for accessing platform-specific preferences
/// This allows the Core project to remain platform-independent
/// </summary>
public interface IPreferencesService
{
    /// <summary>
    /// Gets a string value with a default if not found
    /// </summary>
    string? Get(string key, string? defaultValue = null);

    /// <summary>
    /// Sets a string value
    /// </summary>
    void Set(string key, string? value);

    /// <summary>
    /// Removes a value by key
    /// </summary>
    bool Remove(string key);

    /// <summary>
    /// Checks if a key exists
    /// </summary>
    bool ContainsKey(string key);

    /// <summary>
    /// Gets an integer value with a default if not found
    /// </summary>
    int Get(string key, int defaultValue);

    /// <summary>
    /// Sets an integer value
    /// </summary>
    void Set(string key, int value);

    /// <summary>
    /// Removes all values
    /// </summary>
    void RemoveAll();
}
