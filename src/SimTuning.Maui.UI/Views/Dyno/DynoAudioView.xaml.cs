// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoAudioView : ContentView
    {
        private readonly ILogger<DynoAudioView> _logger;

        public DynoAudioViewModel ViewModel => (DynoAudioViewModel)BindingContext;

        public DynoAudioView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DynoAudioViewModel>();
            _logger = Ioc.Default.GetRequiredService<ILogger<DynoAudioView>>();

            mediaElement.Source = GeneralSettings.AudioAccelerationFilePath;
        }

        private void OnMediaEnded(object? sender, EventArgs e) =>
            _logger.LogInformation("Media ended.");

        private void OnMediaFailed(object? sender, MediaFailedEventArgs e) =>
            _logger.LogError("Media failed. Error: {ErrorMessage}", e.ErrorMessage);

        private void OnMediaOpened(object? sender, EventArgs e) =>
            _logger.LogInformation("Media opened.");

        private void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e) =>
            _logger.LogInformation("Media Position changed to {position}", e.Position);

        private void OnSeekCompleted(object? sender, EventArgs e) =>
            _logger.LogInformation("Media Seek completed.");

        private void OnStateChanged(object? sender, MediaStateChangedEventArgs e) =>
            _logger.LogInformation("Media State Changed. Old State: {PreviousState}, New State: {NewState}", e.PreviousState, e.NewState);
    }
}
