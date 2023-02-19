// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.ViewModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using SimTuning.Core.Models;
    using SimTuning.Core.ModuleLogic;
    using SimTuning.Core.Services;using SimTuning.Maui.UI.Services;
    using SimTuning.Data;
    using SimTuning.Data.Models;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using UnitsNet.Units;
    using SimTuning.Core;

    /// <summary>
    /// UmrechnungViewModel.
    /// </summary>
    public class MotorUmrechnungViewModel : ViewModelBase
    {
        /// <summary> Initializes a new instance of the <see cref="MotorUmrechnungViewModel"/>
        /// class. </summary> <param name="logger"><inheritdoc cref="ILogger"
        /// path="/summary/node()" /></param> <param name="INavigationService"><inheritdoc
        /// cref="INavigationService" path="/summary/node()" /></param
        public MotorUmrechnungViewModel(
            ILogger<MotorUmrechnungViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._vehicleService = vehicleService;

            this.VolumeQuantityUnits = new VolumeQuantity();
            this.LengthQuantityUnits = new LengthQuantity();

            // vordefinieren der nicht model werte
            // TODO: den rest der unmodelt.units definieren! this.UnitAbstandOTlength =
            // this.LengthQuantityUnits.Where(x =>
            // x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            this.DifferenceLengthUnit = this.LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            this.VehicleMotorHubRUnit = this.LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            this.LengthDifferenceToOTUnit = this.LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();

            this.HelperVehicles = new ObservableCollection<VehiclesModel>(_vehicleService.RetrieveVehicles());

            this.InsertDataCommand = new RelayCommand(this.InsertData);

            // Vehicle Creation
            this.Vehicle = new VehiclesModel();
            this.Vehicle.Motor = new MotorModel();
        }

        #region Methods


        /// <summary>
        /// Inserts the data.
        /// </summary>
        protected void InsertData()
        {
            if (this.HelperVehicle.Motor.HubL.HasValue)
            {
                this.VehicleMotorHubL = this.HelperVehicle.Motor.HubL;
                this.OnPropertyChanged(nameof(this.VehicleMotorHubL));
            }

            if (this.HelperVehicle.Motor.PleulL.HasValue)
            {
                this.VehicleMotorPleulL = this.HelperVehicle.Motor.PleulL;
                this.OnPropertyChanged(nameof(this.VehicleMotorPleulL));
            }

            if (this.HelperVehicle.Motor.DeachsierungL.HasValue)
            {
                this.VehicleMotorDeachsierungL = this.HelperVehicle.Motor.DeachsierungL;
                this.OnPropertyChanged(nameof(this.VehicleMotorDeachsierungL));
            }
        }

        /// <summary>
        /// Refreshes the unterschied.
        /// </summary>
        private void RefreshDifference()
        {
            if (this.SteuerzeitVorher.HasValue && this.SteuerzeitNachher.HasValue && this.VehicleMotorPleulL.HasValue && this.VehicleMotorHubR.HasValue && this.VehicleMotorDeachsierungL.HasValue && (KolbenoberkanteChecked || KolbenunterkanteChecked))
            {
                (this.SteuerwinkelVorherOeffnet, this.SteuerwinkelVorherSchließt, this.SteuerwinkelNachherOeffnet, this.SteuerwinkelNachherSchließt) =
                EngineLogic.GetSteuerwinkel(this.SteuerzeitVorher.Value, this.SteuerzeitNachher.Value, this.KolbenoberkanteChecked, this.KolbenunterkanteChecked);

                this.DifferenceDegree = EngineLogic.GetPortTimingDifference(false, this.SteuerzeitVorher.Value, this.SteuerzeitNachher.Value);

                // TODO: verbessern und durschnitt aus öffnen und schließen bilden
                this.DifferenceLength = EngineLogic.GetPortTimingDifference(
                    true,
                    this.SteuerwinkelVorherOeffnet.Value,
                    this.SteuerwinkelNachherOeffnet.Value,
                    UnitsNet.UnitConverter.Convert(
                         this.VehicleMotorPleulL.Value,
                         this.VehicleMotorPleulLUnit.UnitEnumValue,
                         MotorModel.PleulLBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                         this.VehicleMotorHubR.Value,
                         this.VehicleMotorHubRUnit.UnitEnumValue,
                         LengthUnit.Millimeter),
                    UnitsNet.UnitConverter.Convert(
                         this.VehicleMotorDeachsierungL.Value,
                         this.VehicleMotorDeachsierungLUnit.UnitEnumValue,
                         MotorModel.DeachsierungLBaseUnit));
            }
        }

        /// <summary>
        /// Refreshes the kwgrad.
        /// </summary>
        private void RefreshDifferenceToOT()
        {
            if (this.VehicleMotorPleulL.HasValue && this.VehicleMotorHubR.HasValue && this.VehicleMotorDeachsierungL.HasValue && this.DegreeDifferenceToOT.HasValue)
            {
                this.LengthDifferenceToOT = EngineLogic.GetDistanceToOT(
                      UnitsNet.UnitConverter.Convert(
                          this.VehicleMotorPleulL.Value,
                          this.VehicleMotorPleulLUnit.UnitEnumValue,
                          MotorModel.PleulLBaseUnit),
                      UnitsNet.UnitConverter.Convert(
                          this.VehicleMotorHubR.Value,
                          this.VehicleMotorHubRUnit.UnitEnumValue,
                          LengthUnit.Millimeter),
                      UnitsNet.UnitConverter.Convert(
                          this.VehicleMotorDeachsierungL.Value,
                          this.VehicleMotorDeachsierungLUnit.UnitEnumValue,
                          MotorModel.DeachsierungLBaseUnit),
                      this.DegreeDifferenceToOT.Value);
            }
        }

        /// <summary>
        /// Refreshes the hubradius.
        /// </summary>
        private void RefreshHubradius()
        {
            if (this.VehicleMotorHubL.HasValue && this.VehicleMotorPleulL.HasValue && this.VehicleMotorDeachsierungL.HasValue)
            {
                this.VehicleMotorHubR = EngineLogic.GetHubRadius(
                     UnitsNet.UnitConverter.Convert(
                         this.VehicleMotorHubL.Value,
                         this.VehicleMotorHubLUnit.UnitEnumValue,
                         MotorModel.HubLBaseUnit),
                     UnitsNet.UnitConverter.Convert(
                         this.VehicleMotorPleulL.Value,
                         this.VehicleMotorPleulLUnit.UnitEnumValue,
                         MotorModel.PleulLBaseUnit),
                     UnitsNet.UnitConverter.Convert(
                         this.VehicleMotorDeachsierungL.Value,
                         this.VehicleMotorDeachsierungLUnit.UnitEnumValue,
                         MotorModel.DeachsierungLBaseUnit));
            }
        }

        #endregion Methods

        #region Values

        #region private

        private double? _degreeDifferenceToOT;
        private double? _differenceDegree;
        private double? _differenceLength;
        private UnitListItem _differenceLengthUnit;
        private VehiclesModel _helperVehicle;
        private ObservableCollection<VehiclesModel> _helperVehicles;
        private bool _kolbenoberkanteChecked;
        private bool _kolbenunterkanteChecked;
        private double? _lengthDifferenceToOT;
        private UnitListItem _lengthDifferenceToOTUnit;
        private double? _steuerwinkelNachherOeffnet;
        private double? _steuerwinkelNachherSchließt;
        private double? _steuerwinkelVorherOeffnet;
        private double? _steuerwinkelVorherSchließt;
        private double? _steuerzeitNachher;
        private double? _steuerzeitVorher;
        private UnitListItem _unitAbstandOTlength;
        private VehiclesModel _vehicle;
        private double? _vehicleMotorHubR;
        private UnitListItem _vehicleMotorHubRUnit;

        #endregion private

        private readonly ILogger<MotorUmrechnungViewModel> _logger;

        private readonly IVehicleService _vehicleService;

        /// <summary>
        /// Gets or sets the steuerzeit.
        /// </summary>
        /// <value>The steuerzeit.</value>
        public double? DegreeDifferenceToOT
        {
            get => this._degreeDifferenceToOT;
            set
            {
                this.SetProperty(ref this._degreeDifferenceToOT, value);
                this.RefreshDifferenceToOT();
            }
        }

        /// <summary>
        /// Gets or sets the unterschied grad.
        /// </summary>
        /// <value>The unterschied grad.</value>
        public double? DifferenceDegree
        {
            get => this._differenceDegree;
            set => this.SetProperty(ref this._differenceDegree, value);
        }

        /// <summary>
        /// Gets or sets the unterschied mm.
        /// </summary>
        /// <value>The unterschied mm.</value>
        public double? DifferenceLength
        {
            get => this._differenceLength;
            set => this.SetProperty(ref this._differenceLength, value);
        }

        /// <summary>
        /// Gets or sets the unit hub r.
        /// </summary>
        /// <value>The unit hub r.</value>
        public UnitListItem DifferenceLengthUnit
        {
            get => this._differenceLengthUnit;
            set
            {
                this.DifferenceLength = Core.Helpers.Functions.UpdateValue(this.DifferenceLength, this._differenceLengthUnit, value);

                SetProperty(ref this._differenceLengthUnit, value);
            }
        }

        /// <summary>
        /// Gets or sets the helper vehicle.
        /// </summary>
        /// <value>The helper vehicle.</value>
        public VehiclesModel HelperVehicle
        {
            get => this._helperVehicle;
            set => SetProperty(ref _helperVehicle, value);
        }

        /// <summary>
        /// Gets or sets the helper vehicles.
        /// </summary>
        /// <value>The helper vehicles.</value>
        public ObservableCollection<VehiclesModel> HelperVehicles
        {
            get => _helperVehicles;
            set { SetProperty(ref _helperVehicles, value); }
        }

        /// <summary>
        /// Gets or sets the insert data command.
        /// </summary>
        /// <value>The insert data command.</value>
        public IRelayCommand InsertDataCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [kolbenoberkante checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [kolbenoberkante checked]; otherwise, <c>false</c>.
        /// </value>
        public bool KolbenoberkanteChecked
        {
            get => this._kolbenoberkanteChecked;
            set
            {
                this.SetProperty(ref this._kolbenoberkanteChecked, value);
                this.RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [kolbenunterkante checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [kolbenunterkante checked]; otherwise, <c>false</c>.
        /// </value>
        public bool KolbenunterkanteChecked
        {
            get => this._kolbenunterkanteChecked;
            set
            {
                this.SetProperty(ref this._kolbenunterkanteChecked, value);
                this.RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets the abstand o tlength.
        /// </summary>
        /// <value>The abstand o tlength.</value>
        public double? LengthDifferenceToOT
        {
            get => this._lengthDifferenceToOT;
            set => this.SetProperty(ref this._lengthDifferenceToOT, value);
        }

        public UnitListItem LengthDifferenceToOTUnit
        {
            get => this._lengthDifferenceToOTUnit;
            set
            {
                this.LengthDifferenceToOT = Core.Helpers.Functions.UpdateValue(this.LengthDifferenceToOT, this._lengthDifferenceToOTUnit, value);

                SetProperty(ref this._lengthDifferenceToOTUnit, value);
            }
        }

        /// <summary>
        /// Gets the length quantity units.
        /// </summary>
        /// <value>The length quantity units.</value>
        public ObservableCollection<UnitListItem> LengthQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the nachher steuerwinkel oeffnet.
        /// </summary>
        /// <value>The nachher steuerwinkel oeffnet.</value>
        public double? SteuerwinkelNachherOeffnet
        {
            get => this._steuerwinkelNachherOeffnet;
            set => this.SetProperty(ref this._steuerwinkelNachherOeffnet, value);
        }

        /// <summary>
        /// Gets or sets the nachher steuerwinkel schließt.
        /// </summary>
        /// <value>The nachher steuerwinkel schließt.</value>
        public double? SteuerwinkelNachherSchließt
        {
            get => this._steuerwinkelNachherSchließt;
            set => this.SetProperty(ref this._steuerwinkelNachherSchließt, value);
        }

        /// <summary>
        /// Gets or sets the vorher steuerwinkel oeffnet.
        /// </summary>
        /// <value>The vorher steuerwinkel oeffnet.</value>
        public double? SteuerwinkelVorherOeffnet
        {
            get => this._steuerwinkelVorherOeffnet;
            set => this.SetProperty(ref this._steuerwinkelVorherOeffnet, value);
        }

        /// <summary>
        /// Gets or sets the vorher steuerwinkel schließt.
        /// </summary>
        /// <value>The vorher steuerwinkel schließt.</value>
        public double? SteuerwinkelVorherSchließt
        {
            get => this._steuerwinkelVorherSchließt;
            set => this.SetProperty(ref this._steuerwinkelVorherSchließt, value);
        }

        /// <summary>
        /// Gets or sets the nachher steuerzeit.
        /// </summary>
        /// <value>The nachher steuerzeit.</value>
        public double? SteuerzeitNachher
        {
            get => this._steuerzeitNachher;
            set
            {
                this.SetProperty(ref this._steuerzeitNachher, value);
                this.RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets the vorher steuerzeit.
        /// </summary>
        /// <value>The vorher steuerzeit.</value>
        public double? SteuerzeitVorher
        {
            get => this._steuerzeitVorher;
            set
            {
                this.SetProperty(ref this._steuerzeitVorher, value);
                this.RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets the unit abstand o tlength.
        /// </summary>
        /// <value>The unit abstand o tlength.</value>
        public UnitListItem UnitAbstandOTlength
        {
            get => this._unitAbstandOTlength;
            set
            {
                this.LengthDifferenceToOT = Core.Helpers.Functions.UpdateValue(this.LengthDifferenceToOT, this._unitAbstandOTlength, value);

                this.SetProperty(ref this._unitAbstandOTlength, value);
            }
        }

        public VehiclesModel Vehicle
        {
            get => this._vehicle;
            set => SetProperty(ref _vehicle, value);
        }

        public double? VehicleMotorDeachsierungL
        {
            get => this.Vehicle?.Motor.DeachsierungL;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.DeachsierungL = value;
                this.RefreshHubradius();
            }
        }

        public UnitListItem VehicleMotorDeachsierungLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.DeachsierungLUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.DeachsierungLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorDeachsierungL));
            }
        }

        public double? VehicleMotorHubL
        {
            get => this.Vehicle?.Motor.HubL;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.HubL = value;
                this.RefreshHubradius();
            }
        }

        public UnitListItem VehicleMotorHubLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.HubLUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.HubLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorHubL));
            }
        }

        /// <summary>
        /// Gets or sets the hub r.
        /// </summary>
        /// <value>The hub r.</value>
        public double? VehicleMotorHubR
        {
            get => _vehicleMotorHubR;
            set => SetProperty(ref _vehicleMotorHubR, value);
        }

        /// <summary>
        /// Gets or sets the unit hub r.
        /// </summary>
        /// <value>The unit hub r.</value>
        public UnitListItem VehicleMotorHubRUnit
        {
            get => this._vehicleMotorHubRUnit;
            set
            {
                this.VehicleMotorHubR = Core.Helpers.Functions.UpdateValue(this.VehicleMotorHubR, this._vehicleMotorHubRUnit, value);

                SetProperty(ref this._vehicleMotorHubRUnit, value);
            }
        }

        public double? VehicleMotorPleulL
        {
            get => this.Vehicle?.Motor.PleulL;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.PleulL = value;
                this.RefreshHubradius();
            }
        }

        public UnitListItem VehicleMotorPleulLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.PleulLUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.PleulLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorPleulL));
            }
        }

        /// <summary>
        /// Gets the volume quantity units.
        /// </summary>
        /// <value>The volume quantity units.</value>
        public ObservableCollection<UnitListItem> VolumeQuantityUnits { get; }

        #endregion Values
    }
}