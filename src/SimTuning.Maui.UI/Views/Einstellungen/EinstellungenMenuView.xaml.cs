// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Einstellungen;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Einstellungen
{
    public partial class EinstellungenMenuView : ContentPage
    {
        public EinstellungenMenuView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<MenuViewModel>();
        }
        public MenuViewModel ViewModel => (MenuViewModel)BindingContext;

        private async void OnButtonApplicationClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new EinstellungenApplicationView());
        }

        private async void OnButtonVehiclesClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new EinstellungenVehiclesView());
        }
    }
}