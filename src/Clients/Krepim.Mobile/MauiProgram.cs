using CommunityToolkit.Maui;
using Krepim.Mobile.Features.Auth.Services;
using Krepim.Mobile.Features.Auth.ViewModels;
using Krepim.Mobile.Features.Auth.Views;
using Krepim.Mobile.Features.Basket.Services;
using Krepim.Mobile.Features.Basket.ViewModels;
using Krepim.Mobile.Features.Basket.Views;
using Krepim.Mobile.Features.Catalog.Services;
using Krepim.Mobile.Features.Catalog.ViewModels;
using Krepim.Mobile.Features.Catalog.Views;
using Krepim.Mobile.Features.Home.ViewModels;
using Krepim.Mobile.Features.Home.Views;
using Krepim.Mobile.Features.Ordering.Services;
using Krepim.Mobile.Features.Ordering.ViewModels;
using Krepim.Mobile.Features.Ordering.Views;
using Krepim.Mobile.Features.Payment.Services;
using Krepim.Mobile.Features.Payment.ViewModels;
using Krepim.Mobile.Features.Payment.Views;
using Krepim.Mobile.Features.Profile.ViewModels;
using Krepim.Mobile.Features.Profile.Views;
using Krepim.Mobile.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

        #region Connect
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
                           .FirstOrDefault(x => x.EndsWith("appsettings.Development.json"));

        if (resourceName != null)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
                builder.Configuration.AddConfiguration(config);
            }
        }

        var apiUrl = builder.Configuration["ApiGatewayUrl"] ?? "http://127.0.0.1:5078/";

        builder.Services.AddTransient<AuthAndErrorHandler>();
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddHttpMessageHandler<AuthAndErrorHandler>();
        #endregion

        #region Services
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<CatalogService>();
        builder.Services.AddSingleton<InventoryService>();
        builder.Services.AddSingleton<BasketService>();
        builder.Services.AddSingleton<OrderService>();
        builder.Services.AddSingleton<PaymentService>();
        #endregion

        #region ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<CatalogViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<BasketViewModel>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<OrdersDashboardViewModel>();
        builder.Services.AddTransient<MockPayViewModel>();
        #endregion

        #region Views
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<CatalogPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<BasketPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<OrderDetailPage>();
        builder.Services.AddTransient<OrdersDashboardPage>();
        builder.Services.AddTransient<MockPayPage>();
        #endregion

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
