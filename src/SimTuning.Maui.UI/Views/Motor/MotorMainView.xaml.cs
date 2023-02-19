// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Motor
{
    public partial class MotorMainView : ContentPage
    {
        public MotorMainView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<MotorMainViewModel>();
        }

        public MotorMainViewModel ViewModel => (MotorMainViewModel)BindingContext;
    }
}