// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Einstellungen
{
    public partial class EinstellungenVehiclesView : ContentPage
    {
        public EinstellungenVehiclesViewModel ViewModel => (EinstellungenVehiclesViewModel)BindingContext;

        public EinstellungenVehiclesView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<EinstellungenVehiclesViewModel>();
        }
    }
}