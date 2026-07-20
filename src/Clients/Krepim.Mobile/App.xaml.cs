using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Microsoft.Extensions.Logging;

namespace Krepim.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider, ILogger<App> logger)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        TaskExtension.GlobalErrorHandler = (ex) =>
        {
            if (ex is UserMessageException userEx)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Windows.Count > 0 && Windows[0].Page != null)
                        await Windows[0].Page!.DisplayAlertAsync(userEx.Title, userEx.Message, "OK");
                });
            }
            else
                logger.LogError(ex, "Критическая ошибка в фоновой задаче (SafeFireAndForget)");
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}