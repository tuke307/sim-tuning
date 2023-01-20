// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ViewModels.Einlass;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Einlass
{
    public partial class EinlassKanalView : ContentView
    {
        public EinlassKanalView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<KanalViewModel>();
        }

        public KanalViewModel ViewModel => (KanalViewModel)BindingContext;
    }
}