using Krepim.Mobile.Features.Ordering.ViewModels;

namespace Krepim.Mobile.Features.Ordering.Views;

public partial class MyOrdersPage : ContentPage
{
    private readonly MyOrdersViewModel _viewModel;

    public MyOrdersPage(MyOrdersViewModel viewModel)
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