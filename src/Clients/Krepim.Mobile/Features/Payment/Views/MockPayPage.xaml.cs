using Krepim.Mobile.Features.Payment.ViewModels;

namespace Krepim.Mobile.Features.Payment.Views;

public partial class MockPayPage : ContentPage
{
    public MockPayPage(MockPayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}