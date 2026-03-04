using QiblaNow.App.ViewModels;

namespace QiblaNow.App.Pages;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}