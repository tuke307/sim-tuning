// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ViewModels.Dyno;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoDiagnosisView : ContentPage
    {
        public DynoDiagnosisView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DiagnosisViewModel>();
        }

        public DiagnosisViewModel ViewModel => (DiagnosisViewModel)BindingContext;
    }
}