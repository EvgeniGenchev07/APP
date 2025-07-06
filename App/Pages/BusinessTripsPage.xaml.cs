using App.PageModels;

namespace App.Pages
{
    public partial class BusinessTripsPage : ContentPage
    {
        public BusinessTripsPage()
        {
            InitializeComponent();
            BindingContext = new BusinessTripsPageModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Refresh data when page appears
            if (BindingContext is BusinessTripsPageModel viewModel)
            {
                viewModel.LoadBusinessTripsCommand.Execute(null);
            }
        }
    }
} 