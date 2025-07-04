using App.PageModels;

namespace App.Pages;


public partial class RegisterPage : ContentPage
{
    public RegisterPage (RegisterPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }

    private void Button_Focused(object sender, FocusEventArgs e)
    {

    }
}