// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Core.ViewModels.Home;

namespace SimTuning.Maui.UI.Views.Home
{
    public partial class HomeMainView : ContentPage
    {
        public HomeMainView()
        {
            InitializeComponent();
            
            this.BindingContext = Ioc.Default.GetRequiredService<HomeViewModel>();
        }

        public HomeViewModel ViewModel => (HomeViewModel)BindingContext;
    }
}