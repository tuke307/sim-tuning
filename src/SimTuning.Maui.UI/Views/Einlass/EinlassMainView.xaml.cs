// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ViewModels.Einlass;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Einlass
{
    public partial class EinlassMainView : ContentPage
    {
        public EinlassMainView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<MainViewModel>();
        }

        public MainViewModel ViewModel => (MainViewModel)BindingContext;
    }
}