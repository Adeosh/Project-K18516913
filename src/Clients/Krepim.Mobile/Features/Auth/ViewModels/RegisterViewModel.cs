using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Auth.Services;

namespace Krepim.Mobile.Features.Auth.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private bool _isFormatting;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string PhoneNumber { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Password { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ConfirmPassword { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        partial void OnPhoneNumberChanged(string value)
        {
            if (_isFormatting || string.IsNullOrEmpty(value)) return;

            _isFormatting = true;
            PhoneNumber = FormatRussianPhoneNumber(value);
            _isFormatting = false;
        }

        [RelayCommand]
        public async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Почта и пароль обязательны";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            IsLoading = true;
            try
            {
                string? phoneToSend = string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Length < 18
                    ? null
                    : PhoneNumber;

                await _authService.RegisterAsync(Email, Password, phoneToSend);
                await Shell.Current.GoToAsync("//profile");
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
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
        public async Task NavigateToLoginAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private string FormatRussianPhoneNumber(string input)
        {
            var digits = new string(input.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("7") || digits.StartsWith("8"))
            {
                digits = digits.Substring(1);
            }

            if (digits.Length == 0) return string.Empty;

            var formatted = "+7";
            if (digits.Length > 0) formatted += $" ({digits.Substring(0, Math.Min(3, digits.Length))}";
            if (digits.Length > 3) formatted += $") {digits.Substring(3, Math.Min(3, digits.Length - 3))}";
            if (digits.Length > 6) formatted += $"-{digits.Substring(6, Math.Min(2, digits.Length - 6))}";
            if (digits.Length > 8) formatted += $"-{digits.Substring(8, Math.Min(2, digits.Length - 8))}";

            return formatted;
        }
    }
}
