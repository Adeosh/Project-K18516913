using Krepim.Mobile.Features.Auth.Services;
using Krepim.Mobile.Features.Auth.Views;

namespace Krepim.Mobile;

public partial class AppShell : Shell
{
    private readonly AuthService _authService;

    public AppShell(AuthService authService)
	{
		InitializeComponent();

        _authService = authService;
        BindingContext = this;

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));
    }

    public Command LogoutCommand => new Command(async () =>
    {
        await _authService.LogoutAsync();
    });
}
