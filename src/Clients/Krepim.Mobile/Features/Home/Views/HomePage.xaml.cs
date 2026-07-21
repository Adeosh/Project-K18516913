using Krepim.Mobile.Features.Home.ViewModels;

namespace Krepim.Mobile.Features.Home.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}