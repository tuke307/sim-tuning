using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Popups;

public partial class DynoCreationPopup : Popup
{
    public DynoCreationPopup()
    {
        InitializeComponent();

        BindingContext = Ioc.Default.GetRequiredService<VehiclesViewModel>();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Close(ViewModel.Vehicle);
    }

    public VehiclesViewModel ViewModel => (VehiclesViewModel)BindingContext;

}