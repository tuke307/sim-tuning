using CommunityToolkit.Maui.Views;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Popups;

public partial class DynoCreationPopup : Popup<VehiclesModel>
{
    public DynoCreationPopup(VehiclesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await CloseAsync(ViewModel.Vehicle);
    }

    public VehiclesViewModel ViewModel => (VehiclesViewModel)BindingContext;

}
