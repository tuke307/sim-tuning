// Copyright (c) 2025 tuke productions. All rights reserved.
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
    public partial class DynoDiagnosisViewModel : ViewModelBase
    {
        public DynoDiagnosisViewModel(
            ILogger<DynoDiagnosisViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;

            AreaQuantityUnits = new AreaQuantity();
            MassQuantityUnits = new MassQuantity();

            // RefreshPlotCommand is source-generated via [RelayCommand] on RefreshPlot().

            // erste navigation: selected dyno wird requested
            Dyno = Messenger.Send<CurrentDynoRequestMessage>();
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
                DynoVehicleGewicht = helperVehicle.Gewicht;
                OnPropertyChanged(nameof(DynoVehicleGewicht));
            }
        }

        /// <summary>
        /// Refreshes the plot.
        /// </summary>
        [RelayCommand]
        protected void RefreshPlot()
        {
            if (!CheckDynoData())
            {
                return;
            }

            var dyno = Dyno;
            if (dyno == null)
            {
                return;
            }

            try
            {
                List<ObservablePoint> values = new List<ObservablePoint>();

                var maxDrehzahl = dyno.Drehzahl.Max(x => x.Drehzahl);
                var rangeToMaxDrehzahl = dyno.Drehzahl.Take(dyno.Drehzahl.IndexOf(dyno.Drehzahl.FirstOrDefault(x => x.Drehzahl == maxDrehzahl))).ToList();
                rangeToMaxDrehzahl.RemoveAt(0);

                foreach (var item in rangeToMaxDrehzahl)
                {
                    values.Add(new ObservablePoint(
                        item.Drehzahl,
                        DynoLogic.GetLeistung(
                        item.Drehzahl,
                        item.Zeit / 1000,
                        1.2,
                        Cw.GetValueOrDefault(),
                        Gesamtübersetzung.GetValueOrDefault(),
                        0.277,
                        DynoVehicleGewicht.GetValueOrDefault(),
                        0.005,
                        9.81,
                        FrontA.GetValueOrDefault(),
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

                dyno.DynoPS = new List<DynoPsModel>();
                foreach (var item in values)
                {
                    dyno.DynoPS.Add(item.ToDynoPSModel());
                }
                _vehicleService.UpdateOne(dyno);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Fehler bei RefreshPlot: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Überprüft ob wichtige Dyno-Audio-Daten vorhanden sind.
        /// </summary>
        private bool CheckDynoData()
        {
            if (Dyno == null)
            {
                _ = Functions.ShowSnackbarDialogAsync(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

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
        private DynoModel? _dyno;
        private ObservableCollection<ISeries>? _plotStrength;
        private double? _gesamtübersetzung;
        private UnitListItem? _frontAUnit;
        private double? _cw;
        private double? _frontA;

        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; }

        public DynoModel? Dyno
        {
            get => _dyno;
            set => SetProperty(ref _dyno, value);
        }

        public double? DynoEnvironmentLuftdruckP
        {
            get => Dyno?.Environment?.LuftdruckP;
            set
            {
                if (Dyno?.Environment == null)
                {
                    return;
                }

                Dyno.Environment.LuftdruckP = value;
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

        public UnitListItem? FrontAUnit
        {
            get => _frontAUnit;
            set => SetProperty(ref _frontAUnit, value);
        }

        public double? DynoVehicleGewicht
        {
            get => Dyno?.Vehicle?.Gewicht;
            set
            {
                if (Dyno?.Vehicle == null)
                {
                    return;
                }

                Dyno.Vehicle.Gewicht = value;
            }
        }

        public UnitListItem? DynoVehicleGewichtUnit
        {
            get => MassQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Dyno?.Vehicle?.GewichtUnit));
            set
            {
                if (Dyno?.Vehicle == null || value == null)
                {
                    return;
                }

                Dyno.Vehicle.GewichtUnit = (UnitsNet.Units.MassUnit)value.UnitEnumValue;
                OnPropertyChanged(nameof(DynoVehicleGewicht));
            }
        }

        public double? Gesamtübersetzung
        {
            get => _gesamtübersetzung;
            set => SetProperty(ref _gesamtübersetzung, value);
        }

        public ObservableCollection<UnitListItem> MassQuantityUnits { get; }

        public ObservableCollection<ISeries>? PlotStrength
        {
            get => _plotStrength;
            set => SetProperty(ref _plotStrength, value);
        }

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
