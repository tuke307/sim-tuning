// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorVerdichtungView : ContentView
    {
        public MotorVerdichtungView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<MotorVerdichtungViewModel>();
        }

        public MotorVerdichtungViewModel ViewModel => (MotorVerdichtungViewModel)BindingContext;
    }
}