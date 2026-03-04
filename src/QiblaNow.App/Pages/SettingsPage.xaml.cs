using QiblaNow.App.ViewModels;

namespace QiblaNow.App.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
    {
		InitializeComponent();
		BindingContext = viewModel;
	}
}