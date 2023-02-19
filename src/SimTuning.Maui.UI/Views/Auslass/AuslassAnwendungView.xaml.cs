// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Auslass
{
    public partial class AuslassAnwendungView : ContentView
    {
        public AuslassAnwendungView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<AuslassAnwendungViewModel>();
        }

        public AuslassAnwendungViewModel ViewModel => (AuslassAnwendungViewModel)BindingContext;
    }
}