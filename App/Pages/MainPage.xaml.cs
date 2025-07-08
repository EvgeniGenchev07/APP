using App.PageModels;

namespace App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MainPageModel pageModel)
        {
             pageModel.LoadDataAsync();
        }
    }
} 