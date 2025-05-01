using CurrencyMobile.ViewModels;
using Microsoft.Maui.Controls;

namespace CurrencyMobile.Views;

public partial class TradePage : ContentPage
{
    private readonly TradeViewModel _viewModel;

    public TradePage(TradeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadRatesCommand.Execute(null);
    }
}