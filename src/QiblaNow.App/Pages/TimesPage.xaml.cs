using QiblaNow.App.ViewModels;

namespace QiblaNow.App.Pages;

public partial class TimesPage : ContentPage
{
    public TimesPage(TimesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
