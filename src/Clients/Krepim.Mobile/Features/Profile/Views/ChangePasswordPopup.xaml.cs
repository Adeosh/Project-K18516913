using CommunityToolkit.Maui.Views;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Auth.Services;

namespace Krepim.Mobile.Features.Profile.Views;

public partial class ChangePasswordPopup : Popup
{
    private readonly AuthService _authService;

    public ChangePasswordPopup()
    {
        InitializeComponent();

        _authService = IPlatformApplication.Current?.Services.GetRequiredService<AuthService>()
            ?? throw new InvalidOperationException("AuthService не зарегистрирован в DI контейнере.");
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ErrorLabel.Text = "Новые пароли не совпадают";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            await _authService.ChangePasswordAsync(OldPasswordEntry.Text, NewPasswordEntry.Text);
            await CloseAsync();
        }
        catch (ApiException apiEx)
        {
            ErrorLabel.Text = apiEx.ToUserFriendlyMessage();
            ErrorLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = "Ошибка: " + ex.Message;
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}