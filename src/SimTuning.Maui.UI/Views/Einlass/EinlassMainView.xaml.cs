// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Einlass
{
    public partial class EinlassMainView : ContentPage
    {
        public EinlassMainViewModel ViewModel => (EinlassMainViewModel)BindingContext;

        public EinlassMainView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<EinlassMainViewModel>();
        }
    }
}