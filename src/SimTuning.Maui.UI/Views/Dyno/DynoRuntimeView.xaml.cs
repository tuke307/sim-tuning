// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ViewModels.Dyno;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoRuntimeView : ContentPage
    {
        public DynoRuntimeView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<RuntimeViewModel>();
        }

        public RuntimeViewModel ViewModel => (RuntimeViewModel)BindingContext;
    }
}