// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

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
    }
}
