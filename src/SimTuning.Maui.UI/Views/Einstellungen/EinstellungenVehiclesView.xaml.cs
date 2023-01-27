// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Einstellungen;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Einstellungen
{
    public partial class EinstellungenVehiclesView : ContentPage
    {
        public EinstellungenVehiclesView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<VehiclesViewModel>();
        }

        public VehiclesViewModel ViewModel => (VehiclesViewModel)BindingContext;
    }
}