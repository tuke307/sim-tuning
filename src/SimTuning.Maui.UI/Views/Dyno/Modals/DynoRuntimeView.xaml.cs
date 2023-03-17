// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoRuntimeView : ContentPage
    {
        public DynoRuntimeView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DynoRuntimeViewModel>();
        }

        public DynoRuntimeViewModel ViewModel => (DynoRuntimeViewModel)BindingContext;
    }
}