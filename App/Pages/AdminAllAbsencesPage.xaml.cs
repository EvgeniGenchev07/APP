using App.PageModels;

namespace App.Pages;

public partial class AdminAllAbsencesPage : ContentPage
{
    public AdminAllAbsencesPage(AdminAllAbsencesPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminAllAbsencesPageModel pageModel)
        {
            await pageModel.LoadAbsencesAsync();
        }
    }
    private void SelectedIndexChanged(object sender, EventArgs e)
    {
        if (BindingContext is AdminAllAbsencesPageModel viewModel)
        {
            viewModel.FilterAbsenceCommand.Execute(null);
        }
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is AdminAllAbsencesPageModel viewModel)
        {
            viewModel.FilterAbsenceCommand.Execute(null);
        }
    }
}