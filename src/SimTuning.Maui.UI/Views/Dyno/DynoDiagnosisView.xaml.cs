// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoDiagnosisView : ContentView
    {
        public DynoDiagnosisView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DynoDiagnosisViewModel>();
        }

        public DynoDiagnosisViewModel ViewModel => (DynoDiagnosisViewModel)BindingContext;
    }
}