// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SimTuning.Core.Helpers;
using LiveChartsCore;

namespace SimTuning.Core.ViewModels.Dyno
{
    public class SpectrogramViewModel : ViewModelBase
    {
        public SpectrogramViewModel(
            ILogger<SpectrogramViewModel> logger,
            INavigationService INavigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._INavigationService = INavigationService;
            this._vehicleService = vehicleService;

            this.RefreshSpectrogramCommand = new RelayCommand(this.ReloadImageAudioSpectrogram);
            this.SpecificGraphCommand = new RelayCommand(this.SpecificGraph);

            this.ShowBeschleunigungCommand = new AsyncRelayCommand(() => this._INavigationService.Navigate<Dyno.GeschwindigkeitViewModel>());

            this.Frequenzbeginn = 3000;
            this.Frequenzende = 12000;
            this.FilterValue = 2;

            this.Quality = this.Qualitys[1]; // mittel
            this.Colormap = this.Colormaps[0]; // viridis
            this.Intensity = this.Intensitys[4]; // 0.5

            this.Normal_Refresh = true;
            this.Badge_Refresh = false;

            this.FilterPlotCommand = new AsyncRelayCommand(this.FilterPlot);
            this.RefreshAudioFileCommand = new AsyncRelayCommand(this.RefreshAudioFileAsync);
            this.RefreshPlotCommand = new AsyncRelayCommand(this.RefreshPlot);

            //this.ReloadData();
        }

        #region Methods

        /// <summary>
        /// Überprüft ob wichtige Dyno-Audio-Daten vorhanden sind.
        /// </summary>
        private bool CheckDynoData()
        {
            if (!File.Exists(SimTuning.Core.GeneralSettings.AudioAccelerationFilePath))
            {
                Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NOAUDIOFILE"));

                return false;
            }

            if (this.Dyno == null)
            {
                Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            return true;
        }


        /// <summary>
        /// Reloads the data.
        /// </summary>
        /// <param name="mvxReloaderMessage">The MVX reloader message.</param>
        //public void ReloadData(Models.MvxReloaderMessage mvxReloaderMessage = null)
        //{
        //    try
        //    {
        //        this.Dyno = _vehicleService.RetrieveOneActive();
        //    }
        //    catch (Exception exc)
        //    {
        //        _logger.LogError("Fehler beim Laden des Dyno-Datensatz: ", exc);
        //    }
        //}

        /// <summary>
        /// Filters the plot.
        /// </summary>
        protected async Task FilterPlot()
        {
            try
            {
                DynoLogic.GetDrehzahlGraph(areas: true, areaAbstand: this.FilterValue);

                //this.OnPropertyChanged(nameof(this.Graphs));

                //this.OnPropertyChanged(nameof(this.PlotAudio));
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei FilterPlot: ", exc);
            }
        }

        /// <summary>
        /// Opens the file dialog.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        // protected async Task OpenFileDialog(string fileName) { //bool status =
        // false;

        // // zip extrahieren if
        // (File.Exists(SimTuning.Core.GeneralSettings.DataExportFilePath)) {
        // File.Delete(SimTuning.Core.GeneralSettings.DataExportFilePath); } if
        // (File.Exists(SimTuning.Core.GeneralSettings.AudioAccelerationFilePath)) {
        // File.Delete(SimTuning.Core.GeneralSettings.AudioAccelerationFilePath); }
        // ZipFile.ExtractToDirectory(fileName,
        // SimTuning.Core.GeneralSettings.FileDirectory);

        // // wenn Datei ausgewählt //using (FileStream sourceStream = File.Open(fileName,
        // FileMode.OpenOrCreate)) //{ // status =
        // SimTuning.Core.Helpers.AudioUtils.AudioCopy(SimTuning.Core.GeneralSettings.AudioFile,
        // sourceStream); //}

        // //if (status) //{ await this.RefreshAudioFileAsync().ConfigureAwait(true); //}

        // // TODO: only for testing if
        // (File.Exists(SimTuning.Core.GeneralSettings.DataExportFilePath)) { string json
        // = File.ReadAllText(SimTuning.Core.GeneralSettings.DataExportFilePath);
        // DynoModel _dyno = JsonConvert.DeserializeObject<DynoModel>(json); } }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <returns>
        /// <placeholder>A <see cref="Task" /> representing the asynchronous
        /// operation.</placeholder>
        /// </returns>
        protected async Task RefreshAudioFileAsync()
        {
            try
            {
                //var generatedMediaItem = await _mediaManager.Extractor.CreateMediaItem(GeneralSettings.AudioAccelerationFilePath).ConfigureAwait(true);
                //_mediaManager.Queue.Add(generatedMediaItem);
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei OpenFileAsync: ", exc);
            }
        }

        /// <summary>
        /// Refreshes the plot.
        /// </summary>
        /// <returns>
        /// <placeholder>A <see cref="Task" /> representing the asynchronous
        /// operation.</placeholder>
        /// </returns>
        protected async Task RefreshPlot()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            try
            {
                DynoLogic.GetDrehzahlGraph(intensity: this.Intensity);

                this.OnPropertyChanged(nameof(this.PlotAudio));

                //PlotAudio.MouseDown += PlotAudioMouseDown;
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei RefreshPlot: ", exc);
            }
        }

        /// <summary>
        /// Reloads the image audio spectrogram.
        /// </summary>
        /// <returns></returns>
        protected void ReloadImageAudioSpectrogram()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            try
            {
                // Normal_Refresh = true; Badge_Refresh = false;
                int _fftSize;
                switch (this.Quality)
                {
                    case "schlecht":
                        _fftSize = 8192; // 2^13
                        break;

                    case "mittel":
                        _fftSize = 16384; // 2^14
                        break;

                    case "gut":
                        _fftSize = 32768; // 2^15
                        break;

                    case "sehr gut":
                        _fftSize = 65536; // 2^16
                        break;

                    default:
                        _fftSize = 16384;
                        break;
                }

                // TODO: duration from audio
                SKBitmap spec = AudioLogic.GetSpectrogram(
                    audioFile: SimTuning.Core.GeneralSettings.AudioAccelerationFilePath,
                    fftSize: _fftSize,
                    intensity: this.Intensity,
                    colormap: this.Colormap,
                    minFreq: this.Frequenzbeginn / 60,
                    maxFreq: this.Frequenzende / 60,
                    targetWidthPx: /*(int)this.Duration / 10*/1000);

                Stream stream = SimTuning.Core.Converters.Converts.SKBitmapToStream(spec);

                this.DisplayedImage = ImageSource.FromStream(() => stream);
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei ReloadImageAudioSpectrogram: ", exc);
            }
        }

        /// <summary>
        /// Specifics the graph.
        /// </summary>
        protected void SpecificGraph()
        {
            //if (this.Graphs == null || this.Graph == null)
            //{
            //    return;
            //}

            try
            {
                //DynoLogic.Graphauswahl = this.Graphs.IndexOf(this.Graph);
                //DynoLogic.GetDrehzahlGraphFitted(out var drehzahlModels);
                //this.Dyno.Drehzahl = drehzahlModels;

                //_vehicleService.UpdateOne(this.Dyno);

                //this.OnPropertyChanged(nameof(PlotAudio));
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei SpecificGraph: ", exc);
            }
        }

        private void PlotAudioMouseDown(object sender/*, OxyMouseDownEventArgs e*/)
        {
            //PlotAudio.EntfernePunkt(e.Position);
        }

        #endregion Methods

        #region Values

        #region Commands

        private readonly ILogger<SpectrogramViewModel> _logger;

        /// <summary>
        /// Gets or sets the filter plot command.
        /// </summary>
        /// <value>The filter plot command.</value>
        public IAsyncRelayCommand FilterPlotCommand { get; set; }

        /// <summary>
        /// Gets or sets the open file command.
        /// </summary>
        /// <value>The open file command.</value>
        // public IAsyncRelayCommand OpenFileCommand { get; set; }

        /// <summary>
        /// Gets or sets the open file command.
        /// </summary>
        /// <value>The open file command.</value>
        public IAsyncRelayCommand RefreshAudioFileCommand { get; set; }

        /// <summary>
        /// Gets or sets the refresh plot command.
        /// </summary>
        /// <value>The refresh plot command.</value>
        public IAsyncRelayCommand RefreshPlotCommand { get; set; }

        /// <summary>
        /// Gets or sets the refresh spectrogram command.
        /// </summary>
        /// <value>The refresh spectrogram command.</value>
        public IRelayCommand RefreshSpectrogramCommand { get; set; }

        /// <summary>
        /// Gets or sets the specific graph command.
        /// </summary>
        /// <value>The specific graph command.</value>
        public IRelayCommand SpecificGraphCommand { get; set; }

        /// <summary>
        /// Gets or sets the stop command.
        /// </summary>
        /// <value>The stop command.</value>
        // public IAsyncRelayCommand StopCommand { get; set; }

        #endregion Commands

        #region private

        //protected readonly IMediaManager _mediaManager;

        protected readonly INavigationService _INavigationService;
        protected readonly IVehicleService _vehicleService;
        private static readonly List<string> _qualitys = new List<string>() { "schlecht", "mittel", "gut", "sehr gut" };
        private bool _badge_Refresh;

        private Spectrogram.Colormap _colormap;

        private List<Spectrogram.Colormap> _colormaps = Enum.GetValues(typeof(Spectrogram.Colormap)).Cast<Spectrogram.Colormap>().ToList();

        private DynoModel _dyno;

        private int _filterValue;

        private int _frequenzbeginn;

        private int _frequenzende;

        private string _graph;

        private double _intensity;

        private List<double> _intensitys = new List<double>() { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.5, 2.0, 3.0, 4.0, 5.0 };

        private bool _normal_Refresh;

        private string _quality;

        /*= CrossMediaManager.Current;*/

        #endregion private


        private ImageSource _displayedImage;

        public ImageSource DisplayedImage
        {
            get => _displayedImage;
            set => SetProperty(ref _displayedImage, value);
        }

        /// <summary>
        /// Gets the show beschleunigung command.
        /// </summary>
        /// <value>The show beschleunigung command.</value>
        public IAsyncRelayCommand ShowBeschleunigungCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [badge refresh].
        /// </summary>
        /// <value><c>true</c> if [badge refresh]; otherwise, <c>false</c>.</value>
        public bool Badge_Refresh
        {
            get => _badge_Refresh;
            set => SetProperty(ref _badge_Refresh, value);
        }

        /// <summary>
        /// Gets the buffered.
        /// </summary>
        /// <value>The buffered.</value>
        //public int Buffered => Convert.ToInt32(_mediaManager.Buffered.TotalSeconds);

        /// <summary>
        /// Gets or sets the colormap.
        /// </summary>
        /// <value>The colormap.</value>
        public Spectrogram.Colormap Colormap
        {
            get => _colormap;
            set
            {
                SetProperty(ref _colormap, value);

                // Warnung setzen
                Normal_Refresh = false;
                Badge_Refresh = true;
            }
        }

        /// <summary>
        /// Gets or sets the colormaps.
        /// </summary>
        /// <value>The colormaps.</value>
        public List<Spectrogram.Colormap> Colormaps
        {
            get => _colormaps;
            // set => SetProperty(ref _colormaps, value);
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>The current.</value>
        //public IMediaItem Current => _mediaManager.Queue.Current;

        /// <summary>
        /// Gets the current subtitle.
        /// </summary>
        /// <value>The current subtitle.</value>
        //public string CurrentSubtitle => Current.DisplaySubtitle;

        /// <summary>
        /// Gets the current title.
        /// </summary>
        /// <value>The current title.</value>
        //public string CurrentTitle => Current.DisplayTitle;

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public DynoModel Dyno
        {
            get => _dyno;
            set => SetProperty(ref _dyno, value);
        }

        /// <summary>
        /// Wert für den Abstand, um einen Punkt herum. Dieser Wert wird bei der Funktion
        /// der Areas verwendet.
        /// </summary>
        public int FilterValue
        {
            get => this._filterValue;
            set => this.SetProperty(ref this._filterValue, value);
        }

        /// <summary>
        /// Gets or sets the frequenzbeginn.
        /// </summary>
        /// <value>The frequenzbeginn.</value>
        public int Frequenzbeginn
        {
            get => _frequenzbeginn;
            set => SetProperty(ref _frequenzbeginn, value);
        }

        /// <summary>
        /// Gets or sets the frequenzende.
        /// </summary>
        /// <value>The frequenzende.</value>
        public int Frequenzende
        {
            get => _frequenzende;
            set => SetProperty(ref _frequenzende, value);
        }

        /// <summary>
        /// Gets or sets the graph.
        /// </summary>
        /// <value>The graph.</value>
        public string Graph
        {
            get => _graph;
            set
            {
                if (value == null)
                {
                    return;
                }

                SetProperty(ref _graph, value);
                SpecificGraphCommand.Execute(null);
            }
        }

        /// <summary>
        /// Gets the graphs.
        /// </summary>
        /// <value>The graphs.</value>
        public List<string> Graphs
        {
            get => null;//DynoLogic.PlotAudio?.Series?.Select(x => x.Title).ToList();
        }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        public double Intensity
        {
            get => _intensity;
            set => SetProperty(ref _intensity, value);
        }

        /// <summary>
        /// Gets or sets the intensitys.
        /// </summary>
        /// <value>The intensitys.</value>
        public List<double> Intensitys
        {
            get => _intensitys;
            set => SetProperty(ref _intensitys, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [normal refresh].
        /// </summary>
        /// <value><c>true</c> if [normal refresh]; otherwise, <c>false</c>.</value>
        public bool Normal_Refresh
        {
            get => _normal_Refresh;
            set => SetProperty(ref _normal_Refresh, value);
        }

        /// <summary>
        /// Gets the plot audio.
        /// </summary>
        /// <value>The plot audio.</value>
        public ISeries PlotAudio
        {
            get => DynoLogic.PlotAudio;
        }

        /// <summary>
        /// Gets or sets the quality.
        /// </summary>
        /// <value>The quality.</value>
        public string Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }

        /// <summary>
        /// Gets or sets the qualitys. 0 schlecht, 1 mittel, 2 gut, 3 sehr gut.
        /// </summary>
        /// <value>The qualitys.</value>
        public List<string> Qualitys
        {
            get => _qualitys;
            // set => SetProperty(ref _qualitys, value);
        }

        #endregion Values
    }
}