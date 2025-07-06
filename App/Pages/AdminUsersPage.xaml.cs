using App.PageModels;

namespace App.Pages;

public partial class AdminUsersPage : ContentPage
{
    public AdminUsersPage(AdminUsersPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 