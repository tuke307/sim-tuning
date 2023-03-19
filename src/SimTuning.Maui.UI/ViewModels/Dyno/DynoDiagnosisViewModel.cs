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
            this.TemperatureQuantityUnits = new TemperatureQuantity();
            this.PressureQuantityUnits = new PressureQuantity();
            this.MassQuantityUnits = new MassQuantity();

            this.RefreshPlotCommand = new RelayCommand(this.RefreshPlot);

            // erste navigation: selected dyno wird requested
            this.Dyno = Messenger.Send<CurrentDynoRequestMessage>();
            // bei dyno änderung (viewmodel schon geladen)
            Messenger.Register<DynoDiagnosisViewModel, DynoChangedMessage>(this, (r, m) => r.Dyno = m.Value);
        }

        #region Methods

        /// <summary>
        /// Inserts the environment.
        /// </summary>
        public void InsertEnvironment(EnvironmentModel helperEnvironment)
        {
            if (helperEnvironment.LuftdruckP.HasValue)
            {
                this.DynoEnvironmentLuftdruckP = helperEnvironment.LuftdruckP;
                this.OnPropertyChanged(nameof(this.DynoEnvironmentLuftdruckP));
            }

            if (helperEnvironment.TemperaturT.HasValue)
            {
                this.DynoEnvironmentTemperaturT = helperEnvironment.TemperaturT;
                this.OnPropertyChanged(nameof(this.DynoEnvironmentTemperaturT));
            }
        }

        public void InsertVehicle(VehiclesModel helperVehicle)
        {
            //if (this.HelperVehicle?.Gewicht != null)
            //{
            //    this.DynoVehicleGewicht = this.HelperVehicle.Gewicht;
            //    this.OnPropertyChanged(nameof(this.DynoVehicleGewicht));
            //}

            //if (this.HelperVehicle.Dyno.Environment..HasValue)
            //{
            //    this.DynoVehicleCw =
            //this.HelperVehicle.Cw; this.OnPropertyChanged(nameof(this.DynoVehicleCw);
            //}

            //if (this.HelperVehicle.FrontA.HasValue)
            //{
            //    this.DynoVehicleFrontA =
            //this.HelperVehicle.FrontA; this.OnPropertyChanged(() =>
            //this.DynoVehicleFrontA);
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
                var rangeToMaxDrehzahl = Dyno.Drehzahl.Take(Dyno.Drehzahl.IndexOf(Dyno.Drehzahl.FirstOrDefault(x => x.Drehzahl == maxDrehzahl)));

                foreach (var item in rangeToMaxDrehzahl)
                {
                    values.Add(new ObservablePoint(item.Drehzahl, DynoLogic.GetLeistung(item.Drehzahl, item.Zeit, 1.2, 0.8, 32.41, 0.277, 170, 0.005, 9.81, 0)));
                }

                PlotStrength = new LineSeries<ObservablePoint>()
                {
                    GeometrySize = 5,
                    Name = "Leistung",
                    Values = values,
                };

                //this.Dyno.DynoPS = ps;
                //_vehicleService.UpdateOne(this.Dyno);
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

        /// <summary>
        /// Creates new environment.
        /// </summary>
        private void NewEnvironment()
        {
            if (this.Dyno.Environment == null)
            {
                this.Dyno.Environment = new EnvironmentModel()
                {
                    Name = "Automatisch erstellt " + DateTime.Now
                };

                //this.OnPropertyChanged(nameof(this.Dyno));
            }
        }

        #endregion Methods

        #region Values

        protected readonly IVehicleService _vehicleService;
        private readonly ILogger<DynoDiagnosisViewModel> _logger;
        private DynoModel _dyno;
        private ISeries _plotStrength;

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

        public UnitListItem DynoEnvironmentLuftdruckPUnit
        {
            get => this.PressureQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Dyno?.Environment?.LuftdruckPUnit)); set
            {
                if (this.Dyno?.Environment == null) { return; }

                this.Dyno.Environment.LuftdruckPUnit = (UnitsNet.Units.PressureUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.DynoEnvironmentLuftdruckP));
            }
        }

        public double? DynoEnvironmentTemperaturT
        {
            get => Dyno?.Environment?.TemperaturT;
            set
            {
                if (this.Dyno?.Environment == null)
                {
                    return;
                }

                this.Dyno.Environment.TemperaturT = value;
            }
        }

        public UnitListItem DynoEnvironmentTemperaturTUnit
        {
            get => this.TemperatureQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Dyno?.Environment?.TemperaturTUnit)); set
            {
                if (this.Dyno?.Environment == null) { return; }

                this.Dyno.Environment.TemperaturTUnit =
                (UnitsNet.Units.TemperatureUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.DynoEnvironmentTemperaturT));
            }
        }

        //public double? DynoVehicleCw
        //{
        //    get => Dyno?.Vehicle?.Cw; set
        //    {
        //        if
        //(this.Dyno?.Vehicle == null) { return; }

        //        this.Dyno.Vehicle.Cw = value;
        //    }
        //}

        //public double? DynoVehicleFrontA
        //{
        //    get => Dyno?.Vehicle?.FrontA; set
        //    {
        //        if(this.Dyno?.Vehicle == null) { return; }

        //        this.Dyno.Vehicle.FrontA = value;
        //    }
        //}

        //public UnitListItem DynoVehicleFrontAUnit
        //{
        //    get => this.AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Dyno?.Vehicle?.FrontA));
        //    set
        //    {
        //        if(this.Dyno?.Vehicle == null) { return; }

        //        this.Dyno.Vehicle.FrontAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue;
        //        this.OnPropertyChanged(nameof(this.DynoVehicleFrontAUnit));
        //    }
        //}

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

        //public double? DynoVehicleUebersetzung
        //{
        //    get => Dyno?.Vehicle?.Uebersetzung;
        //    set
        //    {
        //        if (this.Dyno?.Vehicle == null) { return; }

        //        this.Dyno.Vehicle.Uebersetzung = value;
        //    }
        //}

        public ObservableCollection<UnitListItem> MassQuantityUnits { get; }

        public ISeries PlotStrength
        {
            get => _plotStrength;
            set => SetProperty(ref _plotStrength, value);
        }

        public ObservableCollection<UnitListItem> PressureQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the refresh plot command.
        /// </summary>
        /// <value>The refresh plot command.</value>
        public IRelayCommand RefreshPlotCommand { get; set; }

        public ObservableCollection<UnitListItem> TemperatureQuantityUnits { get; }

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