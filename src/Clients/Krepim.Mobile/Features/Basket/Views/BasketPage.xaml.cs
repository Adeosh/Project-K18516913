using Krepim.Mobile.Features.Basket.ViewModels;

namespace Krepim.Mobile.Features.Basket.Views;

public partial class BasketPage : ContentPage
{
    private readonly BasketViewModel _viewModel;

    public BasketPage(BasketViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}