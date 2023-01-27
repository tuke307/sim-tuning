// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Motor;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorHubraumView : ContentView
    {
        public MotorHubraumView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<HubraumViewModel>();
        }

        public HubraumViewModel ViewModel => (HubraumViewModel)BindingContext;
    }
}