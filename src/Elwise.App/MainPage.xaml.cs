using Elwise.App.ViewModels;

namespace Elwise.App;

public partial class MainPage : ContentPage
{
    private readonly PricesViewModel _viewModel;

    public MainPage(PricesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.LoadAsync();
        }
        catch (HttpRequestException)
        {
            await DisplayAlertAsync("Ingen forbindelse", "Priserne kunne ikke hentes. Prøv igen senere.", "OK");
        }
    }
}