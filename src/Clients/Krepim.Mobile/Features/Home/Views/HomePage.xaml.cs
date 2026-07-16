using Krepim.Mobile.Features.Home.ViewModels;

namespace Krepim.Mobile.Features.Home.Views;

public partial class HomePage : ContentPage
{
	public HomePage(HomeViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
    }
}