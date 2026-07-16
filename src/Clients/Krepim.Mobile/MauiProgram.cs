using CommunityToolkit.Maui;
using Krepim.Mobile.Features.Auth.Services;
using Krepim.Mobile.Features.Auth.ViewModels;
using Krepim.Mobile.Features.Auth.Views;
using Krepim.Mobile.Features.Home.ViewModels;
using Krepim.Mobile.Features.Home.Views;
using Krepim.Mobile.Http;
using Microsoft.Extensions.Logging;

namespace Krepim.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddTransient<AuthAndErrorHandler>();
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddHttpMessageHandler<AuthAndErrorHandler>();

        #region Services
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<AuthService>();
        #endregion

        #region ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        #endregion

        #region Views
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        #endregion

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
