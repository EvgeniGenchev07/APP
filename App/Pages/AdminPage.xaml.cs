using App.PageModels;

namespace App.Pages;

public partial class AdminPage : ContentPage
{
    public AdminPage(AdminPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    private void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CalendarDay selectedDay)
        {
            ((AdminPageModel)BindingContext).SelectDay(selectedDay);
        }
    }
} 