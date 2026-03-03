using QiblaNow.App.ViewModels;

namespace QiblaNow.App.Pages;

public partial class CompassPage : ContentPage
{
    public CompassPage(CompassViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
