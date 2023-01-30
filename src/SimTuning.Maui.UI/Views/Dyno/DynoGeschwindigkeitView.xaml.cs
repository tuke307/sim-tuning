// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Dyno;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoGeschwindigkeitView : ContentPage
    {
        public DynoGeschwindigkeitView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<GeschwindigkeitViewModel>();
        }

        public GeschwindigkeitViewModel ViewModel => (GeschwindigkeitViewModel)BindingContext;
    }
}