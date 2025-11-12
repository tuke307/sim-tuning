// Copyright (c) 2025 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoGeschwindigkeitView : ContentPage
    {
        public DynoGeschwindigkeitView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DynoGeschwindigkeitViewModel>();
        }

        public DynoGeschwindigkeitViewModel ViewModel => (DynoGeschwindigkeitViewModel)BindingContext;
    }
}