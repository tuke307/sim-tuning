// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;
using SimTuning.Maui.UI.Views.Popups;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorSteuerdiagrammView : ContentView
    {
        public MotorSteuerdiagrammViewModel ViewModel => (MotorSteuerdiagrammViewModel)BindingContext;

        public MotorSteuerdiagrammView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<MotorSteuerdiagrammViewModel>();
        }

        private async void HelperEnginesButton_Clicked(object sender, EventArgs e)
        {
            var page = Window?.Page;
            if (page != null)
            {
                var result = await page.ShowPopupAsync(new PortTimingPopup());
                if (result is MotorModel motorModel)
                {
                    ViewModel.InsertHelperEngines(motorModel);
                }
            }
        }

        private async void HelperVehiclesButton_Clicked(object sender, EventArgs e)
        {
            var page = Window?.Page;
            if (page != null)
            {
                var result = await page.ShowPopupAsync(new VehiclePopup());
                if (result is VehiclesModel vehicleModel)
                {
                    ViewModel.InsertHelperVehicle(vehicleModel);
                }
            }
        }
    }
}
