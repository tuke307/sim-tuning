using CommunityToolkit.Maui.Views;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Popups
{
    public partial class PortTimingPopup : Popup<MotorModel>
    {
        public PortTimingViewModel ViewModel => (PortTimingViewModel)BindingContext;

        public PortTimingPopup(PortTimingViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        private async void Button_Clicked(object? sender, EventArgs e)
        {
            await CloseAsync(ViewModel.Engine!); // justified: popup result — null propagates to the ShowAsync() caller as before
        }
    }
}
