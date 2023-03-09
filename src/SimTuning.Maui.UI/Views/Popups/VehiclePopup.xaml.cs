using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Popups
{
    public partial class VehiclePopup : Popup
    {
        public VehiclesViewModel ViewModel => (VehiclesViewModel)BindingContext;

        public VehiclePopup()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<VehiclesViewModel>();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Close(ViewModel.Vehicle);
        }
    }
}