using CurrencyMobile.ViewModels;

namespace CurrencyMobile.Views;

public partial class RatesPage : ContentPage
{
    private readonly RatesViewModel _viewModel;

    public RatesPage(RatesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCurrentRateCommand.Execute(null);
    }
}
