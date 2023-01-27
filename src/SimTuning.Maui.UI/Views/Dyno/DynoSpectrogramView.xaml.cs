// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Dyno;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoSpectrogramView : ContentPage
    {
        public DynoSpectrogramView()
        {
            InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<SpectrogramViewModel>();
        }

        public SpectrogramViewModel ViewModel => (SpectrogramViewModel)BindingContext;
    }
}