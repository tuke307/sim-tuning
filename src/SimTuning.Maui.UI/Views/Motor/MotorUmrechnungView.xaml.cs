// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorUmrechnungView : ContentView
    {
        public MotorUmrechnungView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<MotorUmrechnungViewModel>();
        }

        public MotorUmrechnungViewModel ViewModel => (MotorUmrechnungViewModel)BindingContext;
    }
}