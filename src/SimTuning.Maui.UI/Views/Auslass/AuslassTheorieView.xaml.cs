// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Auslass
{
    public partial class AuslassTheorieView : ContentView
    {
        public AuslassTheorieView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<AuslassTheorieViewModel>();
        }

        public AuslassTheorieViewModel ViewModel => (AuslassTheorieViewModel)BindingContext;
    }
}