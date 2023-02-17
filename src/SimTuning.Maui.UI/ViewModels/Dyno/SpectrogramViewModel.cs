// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Easing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using MathNet.Numerics;
using Microsoft.Extensions.Logging;
using SimTuning.Core.Helpers;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace SimTuning.Maui.UI.ViewModels.Dyno
{
    public class SpectrogramViewModel : ViewModelBase
    {
        public SpectrogramViewModel(
            ILogger<SpectrogramViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
            this._vehicleService = vehicleService;

            this.Frequenzbeginn = 3000;
            this.Frequenzende = 12000;
            this.FilterValue = 2;

            // letzten 5 sind relevant! von 2^13 bis 2^18
            this.fftSizes = Functions.GetPowersOf2(13, 18);
            this.FftSizeIndex = 2; // 2. Index von fftSizes = 2^15 = 32768
            this.Intensity = 100;

            this.FilterPlotCommand = new AsyncRelayCommand(this.FilterPlot);
            this.RefreshPlotCommand = new AsyncRelayCommand(this.RefreshPlot);
            this.SpecificGraphCommand = new RelayCommand(this.SpecificGraph);

            this.ReloadData();

            //RefreshPlotCommand.ExecuteAsync(null);
        }

        #region Methods

        private List<ObservableCollection<ObservablePoint>> values;

        /// <summary>
        /// Reloads the data.
        /// </summary>
        /// <param name="mvxReloaderMessage">The MVX reloader message.</param>
        public void ReloadData()
        {
            try
            {
                this.Dyno = _vehicleService.RetrieveOneActive();
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler beim Laden des Dyno-Datensatz: ", exc);
            }
        }

        /// <summary>
        /// Filters the plot.
        /// </summary>
        protected async Task FilterPlot()
        {
            try
            {
                LoadFilteredAccelerationGraph();
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei FilterPlot: ", exc);
            }
        }

        /// <summary>
        /// Refreshes the plot.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task" /> representing the asynchronous operation.</placeholder></returns>
        protected async Task RefreshPlot()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            try
            {
                ReloadImageAudioSpectrogram();
                LoadAccelerationGraph();
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei RefreshPlot: ", exc);
            }
        }

        /// <summary>
        /// Reloads the image audio spectrogram.
        /// </summary>
        protected void ReloadImageAudioSpectrogram()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            try
            {
                // TODO: duration from audio
                SKBitmap spec = AudioLogic.GetSpectrogram(
                    audioFile: SimTuning.Core.GeneralSettings.AudioAccelerationFilePath,
                    fftSize: this.fftSizes[FftSizeIndex],
                    intensity: (double)this.Intensity / 100,
                    minFreq: this.Frequenzbeginn / 60,
                    maxFreq: this.Frequenzende / 60,
                    targetWidthPx: /*(int)this.Duration / 10*/1000);

                //Stream stream = SimTuning.Core.Converters.Converts.SKBitmapToStream(spec);
                //this.DisplayedImage = ImageSource.FromStream(() => stream);
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
            if (this.Graphs == null || this.Graph == null)
            {
                return;
            }

            try
            {
                LoadDrehzahlGraphFitted();
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei SpecificGraph: ", exc);
            }
        }

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
        /// Gibt das Diagramm aus den Spectogram Daten zurück. vorher muss Audio-Spectrogram der Audio bereits einmal berechnet sein mit AudioLogic.GetSpectrogram().
        /// </summary>
        private void LoadAccelerationGraph()
        {
            List<ObservablePoint> values = new List<ObservablePoint>();

            AudioLogic.GetDrehzahlGraph(intensity: (double)Intensity / 100);

            foreach (var item in AudioLogic.AccelerationPoints)
            {
                values.Add(item.ToObservablePoint());
            }

            PlotAudio = new ObservableCollection<ISeries>();
            PlotAudio.Add(new ScatterSeries<ObservablePoint>()
            {
                GeometrySize = 5,
                Name = "Acceleration",
                Values = values,
            });
        }

        private void LoadDrehzahlGraphFitted()
        {
            List<ObservablePoint> values = new List<ObservablePoint>();
            int index = Graphs.IndexOf(Graph);
            double xMin = AudioLogic.AreaAccelerationPoints[index].Min(x => x.X);
            double xMax = AudioLogic.AreaAccelerationPoints[index].Max(x => x.X);
            double stepSize = 0.1;
            int polynomialFuncOrder = 5;
            ISeries series = PlotAudio[index];

            PlotAudio.Clear();
            PlotAudio.Add(series);

            // Polynom: Regressions-Punkte bilden
            var drehzahlfunction = Fit.PolynomialFunc(
                AudioLogic.AreaAccelerationPoints[index].Select(x => x.X).ToArray(),
                AudioLogic.AreaAccelerationPoints[index].Select(x => x.Y).ToArray(),
                polynomialFuncOrder);

            for (double x = xMin; x < xMax; x += stepSize)
            {
                values.Add(new ObservablePoint(x, drehzahlfunction(x)));
            }

            PlotAudio.Add(new LineSeries<ObservablePoint>()
            {
                GeometrySize = 5,
                Name = "Eased",
                Values = values,
            });

            //this.Dyno.Drehzahl = values;
            //_vehicleService.UpdateOne(this.Dyno);
        }

        /// <summary>
        /// Definiert Graph für alle Punkte und den Areas.
        /// </summary>
        private void LoadFilteredAccelerationGraph()
        {
            values = new List<ObservableCollection<ObservablePoint>>();

            AudioLogic.GetDrehzahlGraph(areas: true, intensity: (double)Intensity / 100, areaAbstand: this.FilterValue);

            PlotAudio.Clear();

            // spalte einfügen
            for (int anzahl = 0; anzahl < AudioLogic.AreaAccelerationPoints.Count; anzahl++)
            {
                values.Add(new ObservableCollection<ObservablePoint>());

                foreach (var item in AudioLogic.AreaAccelerationPoints[anzahl])
                {
                    values[anzahl].Add(item.ToObservablePoint());
                }

                PlotAudio.Add(new ScatterSeries<ObservablePoint>()
                {
                    GeometrySize = 5,
                    Name = "Graph " + (anzahl + 1),
                    Values = values[anzahl],
                });
            }

            OnPropertyChanged(nameof(Graphs));
        }

        #endregion Methods

        #region Values

        #region Commands

        /// <summary>
        /// Gets or sets the filter plot command.
        /// </summary>
        /// <value>The filter plot command.</value>
        public IAsyncRelayCommand FilterPlotCommand { get; set; }

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
        /// Gets or sets the specific graph command.
        /// </summary>
        /// <value>The specific graph command.</value>
        public IRelayCommand SpecificGraphCommand { get; set; }

        #endregion Commands

        #region private

        protected readonly INavigationService _navigationService;
        protected readonly IVehicleService _vehicleService;
        private readonly ILogger<SpectrogramViewModel> _logger;
        private readonly List<int> fftSizes;
        private DynoModel _dyno;
        private int _fftSizeIndex;
        private int _filterValue;
        private int _frequenzbeginn;
        private int _frequenzende;
        private string _graph;
        private int _intensity;
        private ObservableCollection<ISeries> _plotAudio;

        #endregion private

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
        /// Gets or sets the quality.
        /// </summary>
        /// <value>The quality.</value>
        public int FftSizeIndex
        {
            get => _fftSizeIndex;
            set => SetProperty(ref _fftSizeIndex, value);
        }

        /// <summary>
        /// Wert für den Abstand, um einen Punkt herum. Dieser Wert wird bei der Funktion der Areas verwendet.
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
            get => PlotAudio?.Select(x => x.Name).ToList();
        }

        /// <summary>
        /// Gets or sets the intensity. wird durch 100 dividiert.
        /// </summary>
        /// <value>The intensity.</value>
        public int Intensity
        {
            get => _intensity;
            set => SetProperty(ref _intensity, value);
        }

        /// <summary>
        /// Gets the plot audio.
        /// </summary>
        /// <value>The plot audio.</value>
        public ObservableCollection<ISeries> PlotAudio
        {
            get => _plotAudio;
            set => SetProperty(ref _plotAudio, value);
        }

        public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "Zeit in ms",
                    NamePaint = new SolidColorPaint(SKColors.Black),

                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 },
                },
            };

        public Axis[] YAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "Drehzahl in 1/min",
                    NamePaint = new SolidColorPaint(SKColors.Black),

                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    TextSize = 14,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect(new float[] { 3, 3 }),
                    },
                },
            };

        #endregion Values
    }
}