using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Popups
{
    public partial class PortTimingPopup : Popup
    {
        public PortTimingViewModel ViewModel => (PortTimingViewModel)BindingContext;

        public PortTimingPopup()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<PortTimingViewModel>();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Close(ViewModel.Engine);
        }
    }
}