// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ViewModels.Motor;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorUmrechnungView : ContentView
    {
        public MotorUmrechnungView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<UmrechnungViewModel>();
        }

        public UmrechnungViewModel ViewModel => (UmrechnungViewModel)BindingContext;
    }
}