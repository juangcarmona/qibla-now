using QiblaNow.App.Resources.Localization;

namespace QiblaNow.App.Pages;

public partial class LanguageSettingsPage : ContentPage
{
    private readonly List<(string Flag, string Name, string Code)> _languages;
    private bool _isInitializing;

    public LanguageSettingsPage()
    {
        InitializeComponent();

        _languages =
        [
            ("🌐", AppResources.LanguageSettings_SystemDefault, string.Empty),
            ("🇬🇧", AppResources.LanguageSettings_English,       "en"),
            ("🇪🇸", AppResources.LanguageSettings_Spanish,      "es"),
            ("🇫🇷", "Français",                                "fr"),
            ("🇸🇦", AppResources.LanguageSettings_Arabic,        "ar"),
            ("🇵🇰", "اردو",                                    "ur"),
            ("🇧🇩", "বাংলা",                                   "bn"),
            ("🇮🇩", "Bahasa Indonesia",                        "id"),
            ("🇹🇷", "Türkçe",                                  "tr"),
        ];

        LanguagePicker.ItemsSource = _languages.Select(l => $"{l.Flag} {l.Name}").ToList();

        // Pre-select the currently active language
        _isInitializing = true;
        var saved = LocalizationHelper.GetSavedLanguageCode();
        var index = _languages.FindIndex(l => l.Code == saved);
        LanguagePicker.SelectedIndex = index >= 0 ? index : 0;
        _isInitializing = false;
    }

    private void OnLanguageSelected(object? sender, EventArgs e)
    {
        if (_isInitializing) return;
        if (LanguagePicker.SelectedIndex < 0) return;

        var (_, _, code) = _languages[LanguagePicker.SelectedIndex];

        // Persist and apply the new culture
        LocalizationHelper.SetLanguageCode(code);
        LocalizationHelper.ApplyPersistedCulture();

        // Recreate the root visual tree so all pages reflect the new language and flow direction
        var shell = new AppShell();
        shell.FlowDirection = LocalizationHelper.GetFlowDirection();
        Application.Current!.Windows[0].Page = shell;
    }
}
