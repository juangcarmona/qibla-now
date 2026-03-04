using QiblaNow.Core.Abstractions;

namespace QiblaNow.Core.Services;

/// <summary>
/// Platform-agnostic Preferences service wrapper
/// Uses .NET MAUI Preferences at runtime
/// </summary>
public sealed class PreferencesService : IPreferencesService
{
    public string? Get(string key, string? defaultValue = null)
    {
        try
        {
            return Preferences.Default.Get(key, defaultValue ?? string.Empty);
        }
        catch
        {
            return defaultValue;
        }
    }

    public void Set(string key, string? value)
    {
        try
        {
            Preferences.Default.Set(key, value ?? string.Empty);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public bool Remove(string key)
    {
        try
        {
            Preferences.Remove(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ContainsKey(string key)
    {
        try
        {
            return Preferences.Default.ContainsKey(key);
        }
        catch
        {
            return false;
        }
    }

    public int Get(string key, int defaultValue)
    {
        try
        {
            return Preferences.Default.Get(key, defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }

    public void Set(string key, int value)
    {
        try
        {
            Preferences.Default.Set(key, value);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public void RemoveAll()
    {
        try
        {
            Preferences.Default.Clear();
        }
        catch
        {
            // Ignore storage errors
        }
    }
}
