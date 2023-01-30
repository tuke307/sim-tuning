// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels.Dyno;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoAusrollenView : ContentPage
    {
        public DynoAusrollenView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<AusrollenViewModel>();
        }

        public AusrollenViewModel ViewModel => (AusrollenViewModel)BindingContext;
    }
}