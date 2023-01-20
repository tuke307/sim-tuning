// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ViewModels.Einlass;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Einlass
{
    public partial class EinlassVergaserView : ContentView
    {
        public EinlassVergaserView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<VergaserViewModel>();
        }

        public VergaserViewModel ViewModel => (VergaserViewModel)BindingContext;
    }
}