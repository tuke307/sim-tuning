// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Einlass
{
    public partial class EinlassVergaserView : ContentView
    {
        public EinlassVergaserViewModel ViewModel => (EinlassVergaserViewModel)BindingContext;

        public EinlassVergaserView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<EinlassVergaserViewModel>();
        }
    }
}
