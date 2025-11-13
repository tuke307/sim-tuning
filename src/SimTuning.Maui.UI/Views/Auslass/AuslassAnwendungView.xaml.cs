// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Auslass
{
    public partial class AuslassAnwendungView : ContentView
    {
        public AuslassAnwendungViewModel ViewModel => (AuslassAnwendungViewModel)BindingContext;

        public AuslassAnwendungView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<AuslassAnwendungViewModel>();
        }
    }
}
