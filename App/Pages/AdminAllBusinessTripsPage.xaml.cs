using App.PageModels;

namespace App.Pages;

public partial class AdminAllBusinessTripsPage : ContentPage
{
    public AdminAllBusinessTripsPage(AdminAllBusinessTripsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 