using Microsoft.Maui.Controls;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class MapPage : ContentPage
{
    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
