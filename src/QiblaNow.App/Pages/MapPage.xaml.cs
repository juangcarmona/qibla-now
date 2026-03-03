using QiblaNow.App.ViewModels;

namespace QiblaNow.App.Pages;

public partial class MapPage : ContentPage
{
    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
