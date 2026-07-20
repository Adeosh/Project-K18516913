using CommunityToolkit.Maui.Extensions;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Krepim.Mobile.Features.Profile.ViewModels;

namespace Krepim.Mobile.Features.Profile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;
    private bool _isMapInitialized = false;
    private readonly string _yandexApiKey;

    public ProfilePage(ProfileViewModel viewModel, IConfiguration config)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _yandexApiKey = config["YandexApiKey"] ?? throw new Exception("Ключ Яндекса не найден в настройках!");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProfileCommand.ExecuteAsync(null);

        if (!_isMapInitialized)
        {
            await InitYandexMapAsync();
            _isMapInitialized = true;
        }
        else
        {
            UpdateMapPosition();
        }
    }

    private async Task InitYandexMapAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("map_template.html");
        using var reader = new StreamReader(stream);
        var htmlTemplate = await reader.ReadToEndAsync();

        string lat = _viewModel.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lon = _viewModel.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string address = Uri.EscapeDataString(_viewModel.FullAddress);

        var html = htmlTemplate
            .Replace("{{YandexApiKey}}", _yandexApiKey)
            .Replace("{{lat}}", lat)
            .Replace("{{lon}}", lon)
            .Replace("{{address}}", address);

        MapWebView.Source = new HtmlWebViewSource { Html = html };
    }

    private void UpdateMapPosition()
    {
        string lat = _viewModel.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lon = _viewModel.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string address = Uri.EscapeDataString(_viewModel.FullAddress);

        var script = $"updateMap({lat}, {lon}, '{address}')";
        MapWebView.EvaluateJavaScriptAsync(script);
    }

    private async void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("ymap://location"))
        {
            e.Cancel = true;

            var uri = new Uri(e.Url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            double lat = double.Parse(query["lat"] ?? "0", System.Globalization.CultureInfo.InvariantCulture);
            double lon = double.Parse(query["lon"] ?? "0", System.Globalization.CultureInfo.InvariantCulture);

            Dispatcher.Dispatch(() =>
            {
                _viewModel.Latitude = lat;
                _viewModel.Longitude = lon;
                _viewModel.FullAddress = "Определяем адрес...";
            });

            await FetchAddressNativelyAsync(lat, lon);
        }
    }

    private async Task FetchAddressNativelyAsync(double lat, double lon)
    {
        try
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Referer", "http://localhost/");

            string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);

            string url = $"https://geocode-maps.yandex.ru/1.x/?apikey={_yandexApiKey}&format=json&geocode={lonStr},{latStr}&results=1";

            var json = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var featureMembers = doc.RootElement
                .GetProperty("response")
                .GetProperty("GeoObjectCollection")
                .GetProperty("featureMember");

            string address = "Адрес не определен";

            if (featureMembers.GetArrayLength() > 0)
            {
                var geoObject = featureMembers[0].GetProperty("GeoObject");
                if (geoObject.TryGetProperty("metaDataProperty", out var metaData) &&
                    metaData.TryGetProperty("GeocoderMetaData", out var geocoderMeta) &&
                    geocoderMeta.TryGetProperty("text", out var textElem))
                {
                    address = textElem.GetString() ?? "Адрес не определен";
                }
            }

            Dispatcher.Dispatch(() =>
            {
                _viewModel.FullAddress = address;
                UpdateMapPosition();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка C# геокодера: {ex.Message}");
            Dispatcher.Dispatch(() =>
            {
                _viewModel.FullAddress = "Ошибка сети при определении адреса";
            });
        }
    }

    private void OnChangePasswordClicked(object sender, EventArgs e)
    {
        var popup = new ChangePasswordPopup();
        this.ShowPopup(popup);
    }
}