// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoAudioPlayerView : ContentView
    {
        public DynoAudioPlayerView()
        {
            this.InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<DynoAudioPlayerViewModel>();
        }

        public DynoAudioPlayerViewModel ViewModel => (DynoAudioPlayerViewModel)BindingContext;
    }
}