using Krepim.Mobile.Features.Auth.ViewModels;

namespace Krepim.Mobile.Features.Auth.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
    }
}