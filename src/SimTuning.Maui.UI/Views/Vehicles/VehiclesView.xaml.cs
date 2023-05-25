// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Vehicles
{
    public partial class VehiclesView : ContentPage
    {
        public VehiclesViewModel ViewModel => (VehiclesViewModel)BindingContext;

        public VehiclesView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<VehiclesViewModel>();
        }
    }
}