using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Payment.Models.Exchange;
using Krepim.Mobile.Features.Payment.Services;

namespace Krepim.Mobile.Features.Payment.ViewModels
{
    [QueryProperty(nameof(TransactionId), "tx")]
    [QueryProperty(nameof(OrderId), "orderId")]
    [QueryProperty(nameof(RawAmount), "amount")]
    public partial class MockPayViewModel : ObservableObject
    {
        private readonly PaymentService _paymentService;

        [ObservableProperty]
        public partial string TransactionId { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string OrderId { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string RawAmount { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string SelectedStatus { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        public string DisplayAmount
        {
            get
            {
                var cleanAmount = RawAmount?.Replace(" ", "").Replace(",", ".") ?? "0";
                if (decimal.TryParse(cleanAmount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed.ToString("N0") + " ₽";
                }
                return "0 ₽";
            }
        }

        partial void OnRawAmountChanged(string value) => OnPropertyChanged(nameof(DisplayAmount));

        public MockPayViewModel(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [RelayCommand]
        public async Task SimulatePaymentAsync(string status)
        {
            if (string.IsNullOrEmpty(TransactionId) || string.IsNullOrEmpty(OrderId))
            {
                ErrorMessage = "Неверная ссылка на оплату (отсутствуют параметры).";
                return;
            }

            IsLoading = true;
            SelectedStatus = status;
            ErrorMessage = string.Empty;

            try
            {
                var request = new MockWebhookRequest(TransactionId, status);
                await _paymentService.SimulateMockPaymentAsync(request);

                await Shell.Current.GoToAsync($"OrderDetailPage?orderId={OrderId}");
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
                SelectedStatus = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Произошла ошибка связи с тестовым шлюзом: " + ex.Message;
                SelectedStatus = string.Empty;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task GoBackToOrderAsync()
        {
            await Shell.Current.GoToAsync($"OrderDetailPage?orderId={OrderId}");
        }
    }
}
