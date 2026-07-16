using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Features.Auth.Services;
using Krepim.Mobile.Http;

namespace Krepim.Mobile.Features.Auth.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Password { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Все поля обязательны для заполнения";
                return;
            }

            IsLoading = true;
            try
            {
                await _authService.LoginAsync(Email, Password);
                await Shell.Current.GoToAsync("//home");
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.Problem.Detail ?? apiEx.Problem.Title;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка подключения к серверу: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("//register");
        }
    }
}
