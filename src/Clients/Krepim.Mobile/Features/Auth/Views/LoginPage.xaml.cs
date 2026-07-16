using Krepim.Mobile.Features.Auth.ViewModels;

namespace Krepim.Mobile.Features.Auth.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
    }
}