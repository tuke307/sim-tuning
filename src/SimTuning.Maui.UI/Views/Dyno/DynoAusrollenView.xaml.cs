// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoAusrollenView : ContentPage
    {
        public DynoAusrollenView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<DynoAusrollenViewModel>();
        }

        public DynoAusrollenViewModel ViewModel => (DynoAusrollenViewModel)BindingContext;
    }
}