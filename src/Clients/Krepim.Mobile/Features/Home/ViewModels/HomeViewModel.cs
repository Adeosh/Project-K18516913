using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Home.ViewModels
{
    public class PromoCategory
    {
        public string Title { get; set; } = string.Empty;
        public string ImageSource { get; set; } = string.Empty;
    }

    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<PromoCategory> Categories { get; set; } = new();

        public HomeViewModel()
        {
            Categories = new ObservableCollection<PromoCategory>
            {
                new PromoCategory { Title = "Анкера", ImageSource = "anchors_2.png" },
                new PromoCategory { Title = "Болты", ImageSource = "bolts_2.png" },
                new PromoCategory { Title = "Дюбели", ImageSource = "dubel.png" },
                new PromoCategory { Title = "Гайки", ImageSource = "gaiki.png" },
                new PromoCategory { Title = "Гвозди", ImageSource = "nails_2.png" },
                new PromoCategory { Title = "Шурупы", ImageSource = "arma.png" },
                new PromoCategory { Title = "Саморезы", ImageSource = "selfr.png" },
                new PromoCategory { Title = "Инструменты", ImageSource = "tools.png" }
            };
        }

        [RelayCommand]
        public async Task CategoryTappedAsync(string title)
        {
            await Shell.Current.GoToAsync($"//catalog?search={Uri.EscapeDataString(title)}");
        }
    }
}
