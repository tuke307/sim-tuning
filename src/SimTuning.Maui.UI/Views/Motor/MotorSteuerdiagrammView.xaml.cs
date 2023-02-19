// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorSteuerdiagrammView : ContentView
    {
        public MotorSteuerdiagrammView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<MotorSteuerdiagrammViewModel>();
        }

        public MotorSteuerdiagrammViewModel ViewModel => (MotorSteuerdiagrammViewModel)BindingContext;
    }
}