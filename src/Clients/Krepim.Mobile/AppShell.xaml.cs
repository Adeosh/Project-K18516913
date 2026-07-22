using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Auth.Services;
using Krepim.Mobile.Features.Auth.Views;
using Krepim.Mobile.Features.Catalog.Views;
using Krepim.Mobile.Features.Ordering.Views;
using Krepim.Mobile.Features.Payment.Views;

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
        Routing.RegisterRoute("product", typeof(ProductDetailPage));
        Routing.RegisterRoute("OrderDetailPage", typeof(OrderDetailPage));
        Routing.RegisterRoute("MockPayPage", typeof(MockPayPage));
        Routing.RegisterRoute("OrdersDashboardPage", typeof(OrdersDashboardPage));

        _authService.InitializeAsync().SafeFireAndForget();
    }

    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        if (args.Target.Location.OriginalString.Contains("profile"))
        {
            if (!_authService.IsAuthenticated)
            {
                args.Cancel();

                Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync("login");
                });
            }
        }
    }

    public Command LogoutCommand => new Command(async () =>
    {
        await _authService.LogoutAsync();
    });
}
