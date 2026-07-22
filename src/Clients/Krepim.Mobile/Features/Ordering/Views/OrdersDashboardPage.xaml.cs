using Krepim.Mobile.Features.Ordering.ViewModels;

namespace Krepim.Mobile.Features.Ordering.Views;

public partial class OrdersDashboardPage : ContentPage
{
    private readonly OrdersDashboardViewModel _viewModel;

    public OrdersDashboardPage(OrdersDashboardViewModel viewModel)
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