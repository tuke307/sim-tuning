// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Auslass
{
    public partial class AuslassTheorieView : ContentView
    {
        public AuslassTheorieViewModel ViewModel => (AuslassTheorieViewModel)BindingContext;

        public AuslassTheorieView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<AuslassTheorieViewModel>();
        }
    }
}
