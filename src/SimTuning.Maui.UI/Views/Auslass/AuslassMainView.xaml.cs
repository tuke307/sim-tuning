// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Auslass;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Auslass
{
    public partial class AuslassMainView : ContentPage
    {
        public AuslassMainView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<MainViewModel>();
        }

        public MainViewModel ViewModel => (MainViewModel)BindingContext;
    }
}