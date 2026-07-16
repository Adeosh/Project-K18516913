namespace Krepim.Mobile;

public partial class App : Application
{
    public App()
	{
		InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = activationState!.Context.Services.GetRequiredService<AppShell>();

        return new Window(appShell);
    }
}