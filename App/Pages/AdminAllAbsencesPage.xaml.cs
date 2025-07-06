using App.PageModels;

namespace App.Pages;

public partial class AdminAllAbsencesPage : ContentPage
{
    public AdminAllAbsencesPage(AdminAllAbsencesPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 