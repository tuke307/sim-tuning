// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;
using SimTuning.Maui.UI.Views.Popups;

namespace SimTuning.Maui.UI.Views.Einlass
{
    public partial class EinlassKanalView : ContentView
    {
        public EinlassKanalViewModel ViewModel => (EinlassKanalViewModel)BindingContext;

        public EinlassKanalView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<EinlassKanalViewModel>();
        }

        private async void HelperVehiclesButton_Clicked(object sender, EventArgs e)
        {
            var task = Application.Current.MainPage.ShowPopupAsync(new VehiclePopup());
            var result = await task;
            if (result != null)
            {
                ViewModel.InsertHelperVehicle(result as VehiclesModel);
            }
        }
    }
}