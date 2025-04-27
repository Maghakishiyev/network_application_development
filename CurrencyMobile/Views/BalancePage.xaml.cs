using CurrencyMobile.ViewModels;

namespace CurrencyMobile.Views;

public partial class BalancePage : ContentPage
{
    private readonly BalanceViewModel _viewModel;

    public BalancePage(BalanceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadBalancesCommand.Execute(null);
    }
}
