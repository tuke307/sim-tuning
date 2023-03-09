// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Auslass
{
    public partial class AuslassMainView : ContentPage
    {
        public AuslassMainView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<AuslassMainViewModel>();
        }

        public AuslassMainViewModel ViewModel => (AuslassMainViewModel)BindingContext;
    }
}