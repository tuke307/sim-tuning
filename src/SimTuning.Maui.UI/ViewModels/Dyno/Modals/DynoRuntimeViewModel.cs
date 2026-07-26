// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using SimTuning.Core;
using SimTuning.Core.Helpers;
using SimTuning.Core.Models;
using SimTuning.Core.Services;
using SimTuning.Data;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace SimTuning.Maui.UI.ViewModels
{
    public partial class DynoRuntimeViewModel : ViewModelBase
    {
        public DynoRuntimeViewModel(
            ILogger<DynoRuntimeViewModel> logger,
            INavigationService navigationService,
            //ILocationService locationService,
            IVehicleService vehicleService)
        {
            _logger = logger;
            _navigationService = navigationService;
            _vehicleService = vehicleService;

            // Commands are source-generated via [RelayCommand] on the methods below.
            ShowAudioButtonVis = true;
            StopwatchVis = false;
            CountdownVis = false;

            // Initial state
            CurrentState = PreState;
            StartAccelerationButtonVis = true;
        }

        #region Methods

        /// <summary>
        /// Ends the acceleration asynchronous.
        /// </summary>
        protected async Task EndRunAsync()
        {
            // Stop tracking
            trackingStarted = false;

            // Update UI
            ShowAudioButtonVis = true;
            StopwatchVis = false;
            CurrentState = PostState;
            PageBackColor = stdBg;
            SpeedBackColor = stdSur;

            // Stop timer and stopwatch
            if (stopwatch != null)
            {
                stopwatch.Stop();
                stopwatch.Reset();
            }
            if (timer != null)
            {
                timer.Stop();
            }
        }
        protected void OnCountdownTimedEvent(object? sender, EventArgs e)
        {
            countdownMilliseconds -= 10;
            OnPropertyChanged(nameof(Countdown));

            if (countdownMilliseconds == 0)
            {
                // Timer löschen und verbergen
                var t = timer;
                if (t != null)
                {
                    t.Stop();
                }
                CountdownVis = false;

                trackingStarted = true;

                // StopAccelerationButtonVis = true;

                // Stopwatch starten
                StartStopwatch();
            }
        }

        // if (Dyno.Geschwindigkeit == null) { Dyno.Geschwindigkeit = new List<GeschwindigkeitModel>(); }
        /// <summary>
        /// Reloads the data.
        /// </summary>
        /// <param name="mvxReloaderMessage">The MVX reloader message.</param>
        //public void ReloadData(Models.MvxReloaderMessage mvxReloaderMessage = null)
        //{
        //    try
        //    {
        //        Dyno = _vehicleService.RetrieveOneActive();
        /// <summary>
        /// Called when [stopwatch timed event].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void OnStopwatchTimedEvent(object? sender, EventArgs e)
        {
            var sw = stopwatch;
            var t = timer;
            if (sw == null || t == null)
            {
                return;
            }

            if (!sw.IsRunning)
            {
                t.Stop();
            }
            else
            {
                OnPropertyChanged(nameof(Stopwatch));
            }
        }

        /// <summary>
        /// Resets the dyno data.
        /// </summary>
        protected void ResetDynoData()
        {
            var dyno = Dyno;
            if (dyno == null)
            {
                return;
            }

            _vehicleService.Delete(dyno.Geschwindigkeit.ToList());
            _vehicleService.Delete(dyno.Ausrollen.ToList());

            dyno.Geschwindigkeit.Clear();

            dyno.Ausrollen.Clear();
        }

        /// <summary>
        /// Resets the acceleration.
        /// </summary>
        [RelayCommand]
        protected async Task ResetAcceleration()
        {
            try
            {
                //var loadingDialog = await DisplayAlert(message: SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "MES_LOAD")).ConfigureAwait(false);

                // Timer und Stopwatch stoppen
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    stopwatch.Reset();
                }
                if (timer != null)
                {
                    timer.Stop();
                }

                // tracking und recording stoppen
                //await recorder.StopRecording();
                trackingStarted = false;

                // UI aktualisieren
                OnPropertyChanged(nameof(Stopwatch));
                PageBackColor = System.Drawing.Color.White;
                SpeedBackColor = System.Drawing.Color.White;
                CurrentState = PreState;
                StopwatchVis = false;
                CountdownVis = false;
                StartAccelerationButtonVis = true;
                ShowAudioButtonVis = true;

                // zuletzt probieren die Audio-Aufnahme zu löschen
                File.Delete(GeneralSettings.AudioAccelerationFilePath);
                File.Delete(GeneralSettings.AudioRolloutFilePath);

                //await loadingDialog.DismissAsync().ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Fehler bei ResetBeschleunigung: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Starts the ausrollen.
        /// </summary>
        protected async Task StartAusrollen()
        {
            try
            {
                StopwatchVis = false;

                CurrentState = RolloutState;
                PageBackColor = deepSkyBlue;
                SpeedBackColor = skyBlue;
                trackingStarted = true;

                //await StartRecording().ConfigureAwait(true);

                // trigger bei jeder 1/100 sekunde
                timer = CreateDispatcherTimer(10, OnCountdownTimedEvent);

                // count down from 5000 ms
                countdownMilliseconds = 5000;

                timer.Start();
                CountdownVis = true;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Fehler bei StartAusrollen: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Starts the acceleration.
        /// </summary>
        [RelayCommand]
        protected async Task StartAcceleration()
        {
            if (!await CheckDynoData().ConfigureAwait(true))
            {
                return;
            }

            try
            {
                //var loadingDialog = await DisplayAlert(message: SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "MES_LOAD")).ConfigureAwait(false);

                ResetDynoData();

                CurrentState = AccelerationState;
                PageBackColor = darkSeaGreen;
                SpeedBackColor = seaGreen;

                // es kann nun nicht mehr weiter navigiert werden
                ShowAudioButtonVis = false;

                //await StartRecording().ConfigureAwait(true);

                StartAccelerationButtonVis = false;

                // trigger bei jeder 1/100 sekunde
                timer = CreateDispatcherTimer(10, OnCountdownTimedEvent);

                // count down from 10000 ms / 10s
                countdownMilliseconds = 10000;

                timer.Start();
                CountdownVis = true;

                //await loadingDialog.DismissAsync().ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Fehler bei StartBeschleunigung: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Starts the stopwatch.
        /// </summary>
        protected void StartStopwatch()
        {
            stopwatch = new Stopwatch();

            if (!stopwatch.IsRunning)
            {
                StopwatchVis = true;
                stopwatch.Start();

                // Trigger nei 1/10 s bzw aller 100 ms
                timer = CreateDispatcherTimer(100, OnStopwatchTimedEvent);

                timer.Start();
            }
        }

        /// <summary>
        /// Creates a UI-thread (<see cref="IDispatcherTimer"/>) timer. Replaces the former
        /// <see cref="System.Timers.Timer"/>, whose <c>Elapsed</c> fired on a thread-pool thread and
        /// mutated bound properties off the UI thread. <see cref="Application.Current" />.Dispatcher is
        /// always available in the running app — the only context these commands execute in.
        /// </summary>
        private static IDispatcherTimer CreateDispatcherTimer(double intervalMilliseconds, EventHandler tick)
        {
            var t = Application.Current!.Dispatcher.CreateTimer();
            t.Interval = TimeSpan.FromMilliseconds(intervalMilliseconds);
            t.Tick += tick;

            return t;
        }

        /// <summary>
        /// Überprüft ob wichtige Dyno-Audio-Daten vorhanden sind.
        /// </summary>
        private async Task<bool> CheckDynoData()
        {
            var locationPermission = await Functions.GetPermission<Permissions.LocationWhenInUse>().ConfigureAwait(true);
            if (locationPermission != PermissionStatus.Granted)
            {
                return false;
            }

            var microphonePermission = await Functions.GetPermission<Permissions.Microphone>().ConfigureAwait(true);
            if (microphonePermission != PermissionStatus.Granted)
            {
                return false;
            }

            if (Dyno == null)
            {
                _ = Functions.ShowSnackbarDialogAsync(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            return true;
        }

        #endregion Methods

        #region Values

        #region Commands

        /// <summary>
        /// Gets or sets the close command.
        /// </summary>
        /// <value>The close command.</value>
        // public IAsyncRelayCommand CloseCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the show audio command.
        /// </summary>
        /// <value>The show audio command.</value>
        public IAsyncRelayCommand? ShowSpectrogramCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the stop acceleration command.
        /// </summary>
        /// <value>The stop acceleration command.</value>
        public IAsyncRelayCommand? StopAccelerationCommand { get; protected set; }

        #endregion Commands

        protected static System.Drawing.Color stdBg;
        protected static System.Drawing.Color stdSur;

        protected readonly INavigationService _navigationService;
        private const string AccelerationState = "Vollgas";
        private const string PostState = "Fertig";
        private const string PreState = "Anfahren";
        private const string RolloutState = "Ausrollen";
        private static System.Drawing.Color darkSeaGreen = System.Drawing.Color.DarkSeaGreen;
        private static System.Drawing.Color deepSkyBlue = System.Drawing.Color.DeepSkyBlue;
        private static System.Drawing.Color seaGreen = System.Drawing.Color.SeaGreen;
        private static System.Drawing.Color skyBlue = System.Drawing.Color.SkyBlue;

        private readonly ILocationService? _locationService;
        private readonly ILogger<DynoRuntimeViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private bool _countdownVis;
        private string? _currentState;
        private DynoModel? _dyno;
        private System.Drawing.Color _pageBackColor;
        private bool _showAudioButtonVis;
        private double? _speed;
        private System.Drawing.Color _speedBackColor;
        private bool _startRunButtonVis;

        // private bool _stopRunButtonVis;
        private bool _stopwatchVis;

        private double countdownMilliseconds;

        //private AudioRecorderService recorder;
        private System.Diagnostics.Stopwatch? stopwatch;

        private IDispatcherTimer? timer;
        private bool trackingStarted;

        /// <summary>
        /// Gets the countdown.
        /// </summary>
        /// <value>The countdown.</value>
        public string Countdown
        {
            get => string.Format("{0:D2}:{1:D2}", CountdownTimeSpan.Seconds, CountdownTimeSpan.Milliseconds);
        }

        /// <summary>
        /// Gets the countdown time span.
        /// </summary>
        /// <value>The countdown time span.</value>
        public TimeSpan CountdownTimeSpan
        {
            get => TimeSpan.FromMilliseconds(countdownMilliseconds);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [countdown vis].
        /// </summary>
        /// <value><c>true</c> if [countdown vis]; otherwise, <c>false</c>.</value>
        public bool CountdownVis
        {
            get => _countdownVis;
            protected set => SetProperty(ref _countdownVis, value);
        }

        /// <summary>
        /// Gets or sets the state of the current.
        /// </summary>
        /// <value>The state of the current.</value>
        public string? CurrentState
        {
            get => _currentState;
            protected set => SetProperty(ref _currentState, value);
        }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public DynoModel? Dyno
        {
            get => _dyno;
            set => SetProperty(ref _dyno, value);
        }

        /// <summary>
        /// Gets or sets the color of the page back.
        /// </summary>
        /// <value>The color of the page back.</value>
        public System.Drawing.Color PageBackColor
        {
            get => _pageBackColor;
            set => SetProperty(ref _pageBackColor, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show audio button vis].
        /// </summary>
        /// <value><c>true</c> if [show audio button vis]; otherwise, <c>false</c>.</value>
        public bool ShowAudioButtonVis
        {
            get => _showAudioButtonVis;
            set => SetProperty(ref _showAudioButtonVis, value);
        }

        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public double? Speed
        {
            get
            {
                if (_speed == null)
                {
                    return 0.0;
                }
                else
                {
                    return Math.Round((double)_speed, 2);
                }
            }
            set => SetProperty(ref _speed, value);
        }

        /// <summary>
        /// Gets or sets the color of the speed back.
        /// </summary>
        /// <value>The color of the speed back.</value>
        public System.Drawing.Color SpeedBackColor
        {
            get => _speedBackColor;
            set => SetProperty(ref _speedBackColor, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [start acceleration button vis].
        /// </summary>
        /// <value><c>true</c> if [start acceleration button vis]; otherwise, <c>false</c>.</value>
        public bool StartAccelerationButtonVis
        {
            get => _startRunButtonVis;
            set => SetProperty(ref _startRunButtonVis, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [stop acceleration button vis].
        /// </summary>
        /// <value><c>true</c> if [stop acceleration button vis]; otherwise, <c>false</c>.</value>
        // public bool StopAccelerationButtonVis { get => _stopRunButtonVis; set => SetProperty(ref _stopRunButtonVis, value); }

        /// <summary>
        /// Gets the stopwatch.
        /// </summary>
        /// <value>The stopwatch.</value>
        public string? Stopwatch
        {
            get => string.Format("{0:D2}:{1:D2}:{2:D2}", stopwatch?.Elapsed.Minutes, stopwatch?.Elapsed.Seconds, stopwatch?.Elapsed.Milliseconds);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [stopwatch vis].
        /// </summary>
        /// <value><c>true</c> if [stopwatch vis]; otherwise, <c>false</c>.</value>
        public bool StopwatchVis
        {
            get => _stopwatchVis;
            set => SetProperty(ref _stopwatchVis, value);
        }

        #endregion Values
    }
}
