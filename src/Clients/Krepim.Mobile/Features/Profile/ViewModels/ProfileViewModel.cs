using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Auth.Models.Enums;
using Krepim.Mobile.Features.Auth.Services;
using Krepim.Mobile.Features.Profile.Models.DTOs;
using Krepim.Mobile.Features.Profile.Models.Exchange;

namespace Krepim.Mobile.Features.Profile.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string PhoneNumber { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string FullAddress { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Flat { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double Latitude { get; set; }

        [ObservableProperty]
        public partial double Longitude { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial bool IsAddressFetching { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SuccessMessage { get; set; } = string.Empty;

        public bool IsManager => _authService.CurrentUser?.Role == UserRole.Manager;

        public ProfileViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var profile = await _authService.GetProfileAsync();

                Email = profile.Email;
                PhoneNumber = profile.PhoneNumber ?? string.Empty;
                FullAddress = profile.DefaultAddress?.FullAddress ?? string.Empty;
                Flat = profile.DefaultAddress?.Flat ?? string.Empty;
                Latitude = profile.DefaultAddress?.Latitude ?? 0.0;
                Longitude = profile.DefaultAddress?.Longitude ?? 0.0;

                OnPropertyChanged(nameof(IsManager));
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки профиля: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveProfileAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                var payload = new UpdateProfilePayload(
                    Email,
                    string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber,
                    string.IsNullOrWhiteSpace(FullAddress) ? null : new AddressDto
                    {
                        FullAddress = FullAddress,
                        Latitude = Latitude,
                        Longitude = Longitude,
                        Flat = string.IsNullOrWhiteSpace(Flat) ? null : Flat
                    }
                );

                await _authService.UpdateProfileAsync(payload);
                SuccessMessage = "Изменения сохранены!";
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка сохранения: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task GoToMyOrdersAsync()
        {
            await Shell.Current.GoToAsync("MyOrdersPage");
        }

        [RelayCommand]
        public async Task GoToOrdersDashboardAsync()
        {
            await Shell.Current.GoToAsync("OrdersDashboardPage");
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
        }
    }
}