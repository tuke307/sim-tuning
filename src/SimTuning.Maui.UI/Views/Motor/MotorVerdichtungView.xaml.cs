// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorVerdichtungView : ContentView
    {
        public MotorVerdichtungViewModel ViewModel => (MotorVerdichtungViewModel)BindingContext;

        public MotorVerdichtungView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<MotorVerdichtungViewModel>();
        }
    }
}
