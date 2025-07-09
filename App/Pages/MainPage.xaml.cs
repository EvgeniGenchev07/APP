using App.PageModels;

namespace App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MainPageModel pageModel)
        {
             await pageModel.LoadDataAsync();
        }
    }
} 