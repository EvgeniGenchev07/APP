using App.PageModels;

namespace App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
} 