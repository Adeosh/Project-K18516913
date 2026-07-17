using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Features.Auth.Services;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Home.ViewModels
{
    public class PromoCategory
    {
        public string Title { get; set; } = string.Empty;
        public string ImageSource { get; set; } = string.Empty;
    }

    public class Partner
    {
        public string ImageSource { get; set; } = string.Empty;
    }

    public partial class HomeViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        public partial ObservableCollection<PromoCategory> Categories { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Partner> Partners { get; set; } = new();

        [ObservableProperty]
        public partial bool IsAuthenticated { get; set; }

        [ObservableProperty]
        public partial string DeliveryAddress { get; set; } = "Сосновый Бор, Ракопежское ш., 32";

        public HomeViewModel(AuthService authService)
        {
            _authService = authService;

            IsAuthenticated = _authService.IsAuthenticated;

            Categories = new ObservableCollection<PromoCategory>
            {
                new PromoCategory { Title = "Анкера", ImageSource = "anchors_2.png" },
                new PromoCategory { Title = "Болты", ImageSource = "bolts_2.png" },
                new PromoCategory { Title = "Дюбели", ImageSource = "dubel.png" },
                new PromoCategory { Title = "Гайки", ImageSource = "gaiki.png" },
                new PromoCategory { Title = "Гвозди", ImageSource = "nails_2.png" },
                new PromoCategory { Title = "Шурупы", ImageSource = "arma.png" },
                new PromoCategory { Title = "Саморезы", ImageSource = "selfr.png" },
                new PromoCategory { Title = "Инструмент", ImageSource = "tools.png" }
            };

            string[] partnerImages = new[]
            {
                "boltru.jpg", "metallservice.jpg", "mtk.jpg", "pic.jpg",
                "russconnect.jpg", "volzgskiy.jpg", "zitar.jpg"
            };

            foreach (var img in partnerImages) Partners.Add(new Partner { ImageSource = img });
            foreach (var img in partnerImages) Partners.Add(new Partner { ImageSource = img });
        }

        [RelayCommand]
        public async Task CategoryTappedAsync(string title)
        {
            await Shell.Current.GoToAsync($"//catalog?search={Uri.EscapeDataString(title)}");
        }
    }
}
