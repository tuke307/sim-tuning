using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Popups
{
    public partial class PortTimingPopup : Popup<MotorModel>
    {
        public PortTimingViewModel ViewModel => (PortTimingViewModel)BindingContext;

        public PortTimingPopup()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<PortTimingViewModel>();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await CloseAsync(ViewModel.Engine);
        }
    }
}
