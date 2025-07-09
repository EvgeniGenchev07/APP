using App.PageModels;
using App.ViewModels;
using BusinessLayer;

namespace App.Pages
{
    public partial class BusinessTripDetailsPage : ContentPage
    {
        public static BusinessTripViewModel SelectedBusinessTrip { get; set; }

        public BusinessTripDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            
            if (SelectedBusinessTrip != null)
            {
                BindingContext = new BusinessTripDetailsPageModel(SelectedBusinessTrip);
            }
            else
            {
                DisplayAlert("Грешка", "Командировката не е намерена", "OK");
                Shell.Current.GoToAsync("..");
            }
        }


    }
} 