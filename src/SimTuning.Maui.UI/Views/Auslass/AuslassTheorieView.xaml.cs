// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Auslass;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Auslass
{
    public partial class AuslassTheorieView : ContentView
    {
        public AuslassTheorieView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<TheorieViewModel>();
        }

        public TheorieViewModel ViewModel => (TheorieViewModel)BindingContext;
    }
}