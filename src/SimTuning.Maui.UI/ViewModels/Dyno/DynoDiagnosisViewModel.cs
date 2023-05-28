// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Helpers;
using SimTuning.Core.Models;
using SimTuning.Core.Models.Messages;
using SimTuning.Core.Models.Quantity;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace SimTuning.Maui.UI.ViewModels
{
    public class DynoDiagnosisViewModel : ViewModelBase
    {
        public DynoDiagnosisViewModel(
            ILogger<DynoDiagnosisViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._vehicleService = vehicleService;

            this.AreaQuantityUnits = new AreaQuantity();
            this.MassQuantityUnits = new MassQuantity();

            this.RefreshPlotCommand = new RelayCommand(this.RefreshPlot);

            // erste navigation: selected dyno wird requested
            this.Dyno = Messenger.Send<CurrentDynoRequestMessage>();
            // bei dyno änderung (viewmodel schon geladen)
            Messenger.Register<DynoDiagnosisViewModel, DynoChangedMessage>(this, (r, m) => r.Dyno = m.Value);

            Cw = 0.8;
            Gesamtübersetzung = 14;
            DynoVehicleGewicht = 170;
            FrontA = 0.75;
        }

        #region Methods

        public void InsertVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Gewicht != null)
            {
                this.DynoVehicleGewicht = helperVehicle.Gewicht;
                this.OnPropertyChanged(nameof(this.DynoVehicleGewicht));
            }
        }

        /// <summary>
        /// Refreshes the plot.
        /// </summary>
        protected void RefreshPlot()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            try
            {
                List<ObservablePoint> values = new List<ObservablePoint>();

                var maxDrehzahl = Dyno.Drehzahl.Max(x => x.Drehzahl);
                var rangeToMaxDrehzahl = Dyno.Drehzahl.Take(Dyno.Drehzahl.IndexOf(Dyno.Drehzahl.FirstOrDefault(x => x.Drehzahl == maxDrehzahl))).ToList();
                rangeToMaxDrehzahl.RemoveAt(0);

                foreach (var item in rangeToMaxDrehzahl)
                {
                    values.Add(new ObservablePoint(
                        item.Drehzahl,
                        DynoLogic.GetLeistung(
                        item.Drehzahl,
                        item.Zeit / 1000,
                        1.2,
                        Cw.Value,
                        Gesamtübersetzung.Value,
                        0.277,
                        DynoVehicleGewicht.Value,
                        0.005,
                        9.81,
                        FrontA.Value,
                        0)));
                }

                PlotStrength = new ObservableCollection<ISeries>();
                PlotStrength.Add(
                    new LineSeries<ObservablePoint>()
                    {
                        GeometrySize = 5,
                        Name = "Leistung in PS",
                        Values = values,
                        Fill = null,
                    });

                this.Dyno.DynoPS = new List<DynoPsModel>();
                foreach (var item in values)
                {
                    this.Dyno.DynoPS.Add(item.ToDynoPSModel());
                }
                _vehicleService.UpdateOne(this.Dyno);
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei RefreshPlot: ", exc);
            }
        }

        /// <summary>
        /// Überprüft ob wichtige Dyno-Audio-Daten vorhanden sind.
        /// </summary>
        private bool CheckDynoData()
        {
            if (this.Dyno == null)
            {
                Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Methods

        #region Values

        protected readonly IVehicleService _vehicleService;
        private readonly ILogger<DynoDiagnosisViewModel> _logger;
        private DynoModel _dyno;
        private ObservableCollection<ISeries> _plotStrength;
        private double? _gesamtübersetzung;
        private UnitListItem _frontAUnit;
        private double? _cw;
        private double? _frontA;

        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; }

        public DynoModel Dyno
        {
            get => _dyno;
            set => SetProperty(ref _dyno, value);
        }

        public double? DynoEnvironmentLuftdruckP
        {
            get => Dyno?.Environment?.LuftdruckP;
            set
            {
                if (this.Dyno?.Environment == null)
                {
                    return;
                }

                this.Dyno.Environment.LuftdruckP = value;
            }
        }

        public double? Cw
        {
            get => _cw;
            set => SetProperty(ref _cw, value);
        }

        public double? FrontA
        {
            get => _frontA;
            set => SetProperty(ref _frontA, value);
        }

        public UnitListItem FrontAUnit
        {
            get => _frontAUnit;
            set => SetProperty(ref _frontAUnit, value);
        }

        public double? DynoVehicleGewicht
        {
            get => this.Dyno?.Vehicle?.Gewicht;
            set
            {
                if (this.Dyno?.Vehicle == null)
                {
                    return;
                }

                this.Dyno.Vehicle.Gewicht = value;
            }
        }

        public UnitListItem DynoVehicleGewichtUnit
        {
            get => this.MassQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Dyno?.Vehicle?.GewichtUnit));
            set
            {
                if (this.Dyno?.Vehicle == null)
                {
                    return;
                }

                this.Dyno.Vehicle.GewichtUnit = (UnitsNet.Units.MassUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.DynoVehicleGewicht));
            }
        }

        public double? Gesamtübersetzung
        {
            get => _gesamtübersetzung;
            set => SetProperty(ref _gesamtübersetzung, value);
        }

        public ObservableCollection<UnitListItem> MassQuantityUnits { get; }

        public ObservableCollection<ISeries> PlotStrength
        {
            get => _plotStrength;
            set => SetProperty(ref _plotStrength, value);
        }

        /// <summary>
        /// Gets or sets the refresh plot command.
        /// </summary>
        /// <value>The refresh plot command.</value>
        public IRelayCommand RefreshPlotCommand { get; set; }

        public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "Drehzahl in 1/min",
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
                    Name = "Leistung in PS",
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