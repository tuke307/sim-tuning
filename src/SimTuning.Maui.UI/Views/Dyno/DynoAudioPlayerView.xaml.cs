// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels.Dyno;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoAudioPlayerView : ContentView
    {
        public DynoAudioPlayerView()
        {
            this.InitializeComponent();
            
            BindingContext = Ioc.Default.GetRequiredService<AudioPlayerViewModel>();
        }

        public AudioPlayerViewModel ViewModel => (AudioPlayerViewModel)BindingContext;
    }
}