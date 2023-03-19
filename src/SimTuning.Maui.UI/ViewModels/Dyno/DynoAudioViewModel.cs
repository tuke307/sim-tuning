// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Easing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using MathNet.Numerics;
using Microsoft.Extensions.Logging;
using SimTuning.Core.Helpers;
using SimTuning.Core.Models.Messages;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace SimTuning.Maui.UI.ViewModels
{
    public class DynoAudioViewModel : ViewModelBase
    {
        public DynoAudioViewModel(
            ILogger<DynoAudioViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
            this._vehicleService = vehicleService;

            this.Frequenzbeginn = 3000;
            this.Frequenzende = 12000;
            this.Epsilon = 160;

            // letzten 5 sind relevant! von 2^13 bis 2^18
            this.fftSizes = Functions.GetPowersOf2(13, 18);
            this.FftSizeIndex = 2; // 2. Index von fftSizes = 2^15 = 32768
            this.Intensity = 100;

            this.RefreshPlotCommand = new RelayCommand(this.RefreshPlot);
            this.FilterPlotCommand = new RelayCommand(this.FilterPlot);
            this.SpecificGraphCommand = new RelayCommand(this.SpecificGraph);

            // erste navigation: selected dyno wird requested
            this.Dyno = Messenger.Send<CurrentDynoRequestMessage>();
            // bei dyno änderung (viewmodel schon geladen)
            Messenger.Register<DynoAudioViewModel, DynoChangedMessage>(this, (r, m) => r.Dyno = m.Value);

            // RefreshPlotCommand.ExecuteAsync(null);
        }

        #region Methods

        private List<ObservableCollection<ObservablePoint>> values;

        /// <summary>
        /// Refreshes the plot.
        /// </summary>
        private void RefreshPlot()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            LoadRotationalSpeed();
        }

        /// <summary>
        /// Filters the plot.
        /// </summary>
        protected void FilterPlot()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            LoadClusters();
        }

        /// <summary>
        /// Specifics the graph.
        /// </summary>
        private void SpecificGraph()
        {
            if (this.Graphs == null || this.Graph == null)
            {
                return;
            }

            LoadSpecificCluster();
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
        /// Lädt das Diagramm (Drehzahl in Abhängkeit der Zeit) aus den Spectogram Daten.
        /// </summary>
        private void LoadRotationalSpeed()
        {
            try
            {
                AudioLogic.CalculateSpectrogram(
                    audioFile: SimTuning.Core.GeneralSettings.AudioAccelerationFilePath,
                    fftSize: this.fftSizes[FftSizeIndex],
                    minFreq: this.Frequenzbeginn / 60,
                    maxFreq: this.Frequenzende / 60,
                    intensity: (double)this.Intensity / 100);

                List<ObservablePoint> values = new List<ObservablePoint>();

                AudioLogic.CalculateRotationalSpeed(intensity: (double)Intensity / 100);

                foreach (var item in AudioLogic.RotationalSpeedPoints)
                {
                    values.Add(item.ToObservablePoint());
                }

                PlotAudio = new ObservableCollection<ISeries>();
                PlotAudio.Add(new ScatterSeries<ObservablePoint>()
                {
                    GeometrySize = 5,
                    Name = "Drehzahl",
                    Values = values,
                });
            }
            catch (Exception exc)
            {
                _logger.LogError("LoadRotationalSpeed: ", exc.Message);
            }
        }

        /// <summary>
        /// Funktion versucht aus den Drehzahl-Zeit Daten, verschiedene Clusters zu erkennen.
        /// zuvor muss RefreshPlot() einmal ausgeführt worden sein.
        /// </summary>
        private void LoadClusters()
        {
            try
            {
                AudioLogic.CalculateClusters(this.Epsilon);

                values = new List<ObservableCollection<ObservablePoint>>();

                PlotAudio.Clear();

                // spalte einfügen
                for (int anzahl = 0; anzahl < AudioLogic.ClusterPoints.Count; anzahl++)
                {
                    values.Add(new ObservableCollection<ObservablePoint>());

                    foreach (var item in AudioLogic.ClusterPoints[anzahl])
                    {
                        values[anzahl].Add(item.ToObservablePoint());
                    }

                    PlotAudio.Add(
                        new ScatterSeries<ObservablePoint>()
                        {
                            GeometrySize = 5,
                            Name = "Graph " + (anzahl + 1),
                            Values = values[anzahl],
                        });
                }

                OnPropertyChanged(nameof(Graphs));
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei LoadClusters: ", exc.Message);
            }
        }

        /// <summary>
        /// Funktion betrachtet ein spezifisch ausgewähltes Cluster und fügt zusätzlich eine Regression dieser ein.
        /// </summary>
        private void LoadSpecificCluster()
        {
            try
            {
                List<ObservablePoint> values = new List<ObservablePoint>();
                int index = Graphs.IndexOf(Graph);
                double xMin = AudioLogic.ClusterPoints[index].Min(x => x.X);
                double xMax = AudioLogic.ClusterPoints[index].Max(x => x.X);
                double stepSize = 5;
                int polynomialFuncOrder = 5;
                ISeries series = PlotAudio[index];

                PlotAudio.Clear();
                PlotAudio.Add(series);

                // Polynom: Regressions-Punkte bilden
                var drehzahlfunction = Fit.PolynomialFunc(
                    AudioLogic.ClusterPoints[index].Select(x => x.X).ToArray(),
                    AudioLogic.ClusterPoints[index].Select(x => x.Y).ToArray(),
                    polynomialFuncOrder);

                for (double x = xMin; x < xMax; x += stepSize)
                {
                    values.Add(new ObservablePoint(x, drehzahlfunction(x)));
                }

                PlotAudio.Add(
                    new LineSeries<ObservablePoint>()
                    {
                        GeometrySize = 5,
                        Name = "Eased",
                        Values = values,
                        Fill = null,
                    });

                this.Dyno.Drehzahl = new List<DrehzahlModel>();
                foreach (var value in values)
                {
                    this.Dyno.Drehzahl.Add(value.ToDrehzahlModel());
                }
                _vehicleService.UpdateOne(this.Dyno);
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei LoadSpecificCluster: ", exc.Message);
            }
        }

        #endregion Methods

        #region Values

        #region Commands

        /// <summary>
        /// Gets or sets the filter plot command.
        /// </summary>
        /// <value>The filter plot command.</value>
        public IRelayCommand FilterPlotCommand { get; set; }

        /// <summary>
        /// Gets or sets the open file command.
        /// </summary>
        /// <value>The open file command.</value>
        public IAsyncRelayCommand RefreshAudioFileCommand { get; set; }

        /// <summary>
        /// Gets or sets the refresh plot command.
        /// </summary>
        /// <value>The refresh plot command.</value>
        public IRelayCommand RefreshPlotCommand { get; set; }

        /// <summary>
        /// Gets or sets the specific graph command.
        /// </summary>
        /// <value>The specific graph command.</value>
        public IRelayCommand SpecificGraphCommand { get; set; }

        #endregion Commands

        #region private

        protected readonly INavigationService _navigationService;
        protected readonly IVehicleService _vehicleService;
        private readonly ILogger<DynoAudioViewModel> _logger;
        private readonly List<int> fftSizes;
        private DynoModel _dyno;
        private int _fftSizeIndex;
        private int _epsilon;
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
        /// Wert für den Abstand, um einen Punkt herum.
        /// Dieser Wert wird bei der Funktion der Cluster verwendet.
        /// </summary>
        public int Epsilon
        {
            get => this._epsilon;
            set => this.SetProperty(ref this._epsilon, value);
        }

        /// <summary>
        /// Frequenzbeginn in U/min.
        /// </summary>
        /// <value>The frequenzbeginn.</value>
        public int Frequenzbeginn
        {
            get => _frequenzbeginn;
            set => SetProperty(ref _frequenzbeginn, value);
        }

        /// <summary>
        /// Frequenzende  in U/min.
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