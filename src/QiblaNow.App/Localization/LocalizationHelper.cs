using System.Globalization;
using Microsoft.Maui.Storage;

namespace QiblaNow.App;

/// <summary>
/// Manages language preference persistence and applies the selected culture
/// at app startup. Isolated from Core/ViewModel layers.
/// </summary>
public static class LocalizationHelper
{
    private const string LanguageKey = "app_language";

    /// <summary>Returns the persisted BCP-47 language code, or empty string for system default.</summary>
    public static string GetSavedLanguageCode()
        => Preferences.Default.Get(LanguageKey, string.Empty);

    /// <summary>Persists the selected language code. Pass empty string to restore system default.</summary>
    public static void SetLanguageCode(string code)
        => Preferences.Default.Set(LanguageKey, code);

    /// <summary>
    /// Reads the persisted language preference and applies it to the current thread
    /// and all future threads. Call this as early as possible in app startup,
    /// before any XAML is inflated.
    /// </summary>
    public static void ApplyPersistedCulture()
    {
        var code = GetSavedLanguageCode();
        if (string.IsNullOrEmpty(code))
            return; // Use OS / system culture

        try
        {
            var culture = new CultureInfo(code);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch (CultureNotFoundException)
        {
            // Stored code is invalid — ignore and fall back to system
        }
    }

    /// <summary>
    /// Returns the MAUI FlowDirection for the given language code, or for the
    /// currently active UI culture when code is null/empty.
    /// </summary>
    public static FlowDirection GetFlowDirection(string? code = null)
    {
        try
        {
            var effectiveCode = string.IsNullOrEmpty(code) ? GetSavedLanguageCode() : code;

            var culture = string.IsNullOrEmpty(effectiveCode)
                ? CultureInfo.CurrentUICulture
                : new CultureInfo(effectiveCode);

            return culture.TextInfo.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }
        catch
        {
            return FlowDirection.LeftToRight;
        }
    }
}
