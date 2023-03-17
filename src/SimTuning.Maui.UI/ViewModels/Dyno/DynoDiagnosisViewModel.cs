// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Helpers;
using SimTuning.Core.Models;
using SimTuning.Core.Models.Quantity;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
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
            //this._messenger = messenger;

            this.AreaQuantityUnits = new AreaQuantity();
            this.TemperatureQuantityUnits = new TemperatureQuantity();
            this.PressureQuantityUnits = new PressureQuantity();
            this.MassQuantityUnits = new MassQuantity();

            this.RefreshPlotCommand = new RelayCommand(this.RefreshPlot);
            //this.ReloadData();
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

        /// <summary>
        /// Reloads the data.
        /// </summary>
        /// <param name="mvxReloaderMessage">The MVX reloader message.</param>
        //protected void ReloadData(Models.MvxReloaderMessage mvxReloaderMessage = null)
        //{
        //    try
        //    {
        //        OnPropertyChanged(nameof(HelperVehicles));
        //        OnPropertyChanged(nameof(HelperEnvironments));
        //        this.Dyno = _vehicleService.RetrieveOneActive();
        //    }
        //    catch (Exception exc)
        //    {
        //        _logger.LogError("Fehler bei ReloadData: ", exc);
        //    }
        //}

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
                //var loadingDialog = await DisplayAlert(message: SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "MES_LOAD")).ConfigureAwait(false);

                //DynoLogic.GetLeistungsGraph(this.Dyno.Vehicle.Gewicht.Value, out List<DynoPsModel> ps/*, out List<DynoNmModel> nm*/);
                //this.Dyno.DynoPS = ps;

                _vehicleService.UpdateOne(this.Dyno);

                this.OnPropertyChanged(nameof(this.PlotStrength));

                //await loadingDialog.DismissAsync().ConfigureAwait(false);
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
            get => null;//DynoLogic.PlotLeistung;
        }

        public ObservableCollection<UnitListItem> PressureQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the refresh plot command.
        /// </summary>
        /// <value>The refresh plot command.</value>
        public IRelayCommand RefreshPlotCommand { get; set; }

        /// <summary>
        /// Gets or sets the show save command.
        /// </summary>
        /// <value>The show save command.</value>
        public IRelayCommand ShowSaveCommand { get; set; }

        public ObservableCollection<UnitListItem> TemperatureQuantityUnits { get; }

        #endregion Values
    }
}