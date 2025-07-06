using App.PageModels;
using BusinessLayer;

namespace App.Pages
{
    public partial class BusinessTripDetailsPage : ContentPage
    {
        public static BusinessTrip SelectedBusinessTrip { get; set; }

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