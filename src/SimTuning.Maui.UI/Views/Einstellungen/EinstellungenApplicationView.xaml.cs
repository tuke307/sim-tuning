// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Einstellungen
{
    public partial class EinstellungenApplicationView : ContentPage
    {
        public EinstellungenApplicationView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<EinstellungenApplicationViewModel>();
        }

        public EinstellungenApplicationViewModel ViewModel => (EinstellungenApplicationViewModel)BindingContext;
    }
}