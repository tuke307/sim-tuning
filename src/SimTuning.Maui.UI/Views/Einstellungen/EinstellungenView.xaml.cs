// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Einstellungen
{
    public partial class EinstellungenView : ContentPage
    {
        public EinstellungenView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<EinstellungenViewModel>();
        }

        public EinstellungenViewModel ViewModel => (EinstellungenViewModel)BindingContext;
    }
}