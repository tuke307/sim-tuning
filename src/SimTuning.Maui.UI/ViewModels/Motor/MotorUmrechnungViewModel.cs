// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.ViewModels
{
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Extensions.Logging;
    using SimTuning.Core;
    using SimTuning.Core.Models;
    using SimTuning.Core.ModuleLogic;
    using SimTuning.Core.Services;
    using SimTuning.Data.Models;
    using SimTuning.Maui.UI.Services;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UnitsNet.Units;

    /// <summary>
    /// UmrechnungViewModel.
    /// </summary>
    public class MotorUmrechnungViewModel : ViewModelBase
    {
        public MotorUmrechnungViewModel(
            ILogger<MotorUmrechnungViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;

            VolumeQuantityUnits = new VolumeQuantity();
            LengthQuantityUnits = new LengthQuantity();

            // vordefinieren der nicht model werte
            // TODO: den rest der unmodelt.units definieren! UnitAbstandOTlength = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            DifferenceLengthUnit = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            VehicleMotorHubRUnit = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            LengthDifferenceToOTUnit = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();

            // Vehicle Creation
            Vehicle = new VehiclesModel();
            Vehicle.Motor = new MotorModel();
        }

        #region Methods

        /// <summary>
        /// Inserts the data.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.HubL.HasValue)
            {
                VehicleMotorHubL = helperVehicle.Motor.HubL;
                OnPropertyChanged(nameof(VehicleMotorHubL));
            }

            if (helperVehicle.Motor.PleulL.HasValue)
            {
                VehicleMotorPleulL = helperVehicle.Motor.PleulL;
                OnPropertyChanged(nameof(VehicleMotorPleulL));
            }

            if (helperVehicle.Motor.DeachsierungL.HasValue)
            {
                VehicleMotorDeachsierungL = helperVehicle.Motor.DeachsierungL;
                OnPropertyChanged(nameof(VehicleMotorDeachsierungL));
            }
        }

        /// <summary>
        /// Refreshes the unterschied.
        /// </summary>
        private void RefreshDifference()
        {
            if (SteuerzeitVorher.HasValue && SteuerzeitNachher.HasValue && VehicleMotorPleulL.HasValue && VehicleMotorHubR.HasValue && VehicleMotorDeachsierungL.HasValue && (KolbenoberkanteChecked || KolbenunterkanteChecked))
            {
                (SteuerwinkelVorherOeffnet, SteuerwinkelVorherSchließt, SteuerwinkelNachherOeffnet, SteuerwinkelNachherSchließt) =
                EngineLogic.GetSteuerwinkel(SteuerzeitVorher.Value, SteuerzeitNachher.Value, KolbenoberkanteChecked, KolbenunterkanteChecked);

                DifferenceDegree = EngineLogic.GetPortTimingDifference(false, SteuerzeitVorher.Value, SteuerzeitNachher.Value);

                // TODO: verbessern und durschnitt aus öffnen und schließen bilden
                DifferenceLength = EngineLogic.GetPortTimingDifference(
                    true,
                    SteuerwinkelVorherOeffnet.Value,
                    SteuerwinkelNachherOeffnet.Value,
                    UnitsNet.UnitConverter.Convert(
                         VehicleMotorPleulL.Value,
                         VehicleMotorPleulLUnit.UnitEnumValue,
                         MotorModel.PleulLBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                         VehicleMotorHubR.Value,
                         VehicleMotorHubRUnit.UnitEnumValue,
                         LengthUnit.Millimeter),
                    UnitsNet.UnitConverter.Convert(
                         VehicleMotorDeachsierungL.Value,
                         VehicleMotorDeachsierungLUnit.UnitEnumValue,
                         MotorModel.DeachsierungLBaseUnit));
            }
        }

        /// <summary>
        /// Refreshes the kwgrad.
        /// </summary>
        private void RefreshDifferenceToOT()
        {
            if (VehicleMotorPleulL.HasValue && VehicleMotorHubR.HasValue && VehicleMotorDeachsierungL.HasValue && DegreeDifferenceToOT.HasValue)
            {
                LengthDifferenceToOT = EngineLogic.GetDistanceToOT(
                      UnitsNet.UnitConverter.Convert(
                          VehicleMotorPleulL.Value,
                          VehicleMotorPleulLUnit.UnitEnumValue,
                          MotorModel.PleulLBaseUnit),
                      UnitsNet.UnitConverter.Convert(
                          VehicleMotorHubR.Value,
                          VehicleMotorHubRUnit.UnitEnumValue,
                          LengthUnit.Millimeter),
                      UnitsNet.UnitConverter.Convert(
                          VehicleMotorDeachsierungL.Value,
                          VehicleMotorDeachsierungLUnit.UnitEnumValue,
                          MotorModel.DeachsierungLBaseUnit),
                      DegreeDifferenceToOT.Value);
            }
        }

        /// <summary>
        /// Refreshes the hubradius.
        /// </summary>
        private void RefreshHubradius()
        {
            if (VehicleMotorHubL.HasValue && VehicleMotorPleulL.HasValue && VehicleMotorDeachsierungL.HasValue)
            {
                VehicleMotorHubR = EngineLogic.GetHubRadius(
                     UnitsNet.UnitConverter.Convert(
                         VehicleMotorHubL.Value,
                         VehicleMotorHubLUnit.UnitEnumValue,
                         MotorModel.HubLBaseUnit),
                     UnitsNet.UnitConverter.Convert(
                         VehicleMotorPleulL.Value,
                         VehicleMotorPleulLUnit.UnitEnumValue,
                         MotorModel.PleulLBaseUnit),
                     UnitsNet.UnitConverter.Convert(
                         VehicleMotorDeachsierungL.Value,
                         VehicleMotorDeachsierungLUnit.UnitEnumValue,
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
            get => _degreeDifferenceToOT;
            set
            {
                SetProperty(ref _degreeDifferenceToOT, value);
                RefreshDifferenceToOT();
            }
        }

        /// <summary>
        /// Gets or sets the unterschied grad.
        /// </summary>
        /// <value>The unterschied grad.</value>
        public double? DifferenceDegree
        {
            get => _differenceDegree;
            set => SetProperty(ref _differenceDegree, value);
        }

        /// <summary>
        /// Gets or sets the unterschied mm.
        /// </summary>
        /// <value>The unterschied mm.</value>
        public double? DifferenceLength
        {
            get => _differenceLength;
            set => SetProperty(ref _differenceLength, value);
        }

        /// <summary>
        /// Gets or sets the unit hub r.
        /// </summary>
        /// <value>The unit hub r.</value>
        public UnitListItem DifferenceLengthUnit
        {
            get => _differenceLengthUnit;
            set
            {
                DifferenceLength = Core.Helpers.Functions.UpdateValue(DifferenceLength, _differenceLengthUnit, value);

                SetProperty(ref _differenceLengthUnit, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [kolbenoberkante checked].
        /// </summary>
        /// <value><c>true</c> if [kolbenoberkante checked]; otherwise, <c>false</c>.</value>
        public bool KolbenoberkanteChecked
        {
            get => _kolbenoberkanteChecked;
            set
            {
                SetProperty(ref _kolbenoberkanteChecked, value);
                RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [kolbenunterkante checked].
        /// </summary>
        /// <value><c>true</c> if [kolbenunterkante checked]; otherwise, <c>false</c>.</value>
        public bool KolbenunterkanteChecked
        {
            get => _kolbenunterkanteChecked;
            set
            {
                SetProperty(ref _kolbenunterkanteChecked, value);
                RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets the abstand o tlength.
        /// </summary>
        /// <value>The abstand o tlength.</value>
        public double? LengthDifferenceToOT
        {
            get => _lengthDifferenceToOT;
            set => SetProperty(ref _lengthDifferenceToOT, value);
        }

        public UnitListItem LengthDifferenceToOTUnit
        {
            get => _lengthDifferenceToOTUnit;
            set
            {
                LengthDifferenceToOT = Core.Helpers.Functions.UpdateValue(LengthDifferenceToOT, _lengthDifferenceToOTUnit, value);

                SetProperty(ref _lengthDifferenceToOTUnit, value);
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
            get => _steuerwinkelNachherOeffnet;
            set => SetProperty(ref _steuerwinkelNachherOeffnet, value);
        }

        /// <summary>
        /// Gets or sets the nachher steuerwinkel schließt.
        /// </summary>
        /// <value>The nachher steuerwinkel schließt.</value>
        public double? SteuerwinkelNachherSchließt
        {
            get => _steuerwinkelNachherSchließt;
            set => SetProperty(ref _steuerwinkelNachherSchließt, value);
        }

        /// <summary>
        /// Gets or sets the vorher steuerwinkel oeffnet.
        /// </summary>
        /// <value>The vorher steuerwinkel oeffnet.</value>
        public double? SteuerwinkelVorherOeffnet
        {
            get => _steuerwinkelVorherOeffnet;
            set => SetProperty(ref _steuerwinkelVorherOeffnet, value);
        }

        /// <summary>
        /// Gets or sets the vorher steuerwinkel schließt.
        /// </summary>
        /// <value>The vorher steuerwinkel schließt.</value>
        public double? SteuerwinkelVorherSchließt
        {
            get => _steuerwinkelVorherSchließt;
            set => SetProperty(ref _steuerwinkelVorherSchließt, value);
        }

        /// <summary>
        /// Gets or sets the nachher steuerzeit.
        /// </summary>
        /// <value>The nachher steuerzeit.</value>
        public double? SteuerzeitNachher
        {
            get => _steuerzeitNachher;
            set
            {
                SetProperty(ref _steuerzeitNachher, value);
                RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets the vorher steuerzeit.
        /// </summary>
        /// <value>The vorher steuerzeit.</value>
        public double? SteuerzeitVorher
        {
            get => _steuerzeitVorher;
            set
            {
                SetProperty(ref _steuerzeitVorher, value);
                RefreshDifference();
            }
        }

        /// <summary>
        /// Gets or sets the unit abstand o tlength.
        /// </summary>
        /// <value>The unit abstand o tlength.</value>
        public UnitListItem UnitAbstandOTlength
        {
            get => _unitAbstandOTlength;
            set
            {
                LengthDifferenceToOT = Core.Helpers.Functions.UpdateValue(LengthDifferenceToOT, _unitAbstandOTlength, value);

                SetProperty(ref _unitAbstandOTlength, value);
            }
        }

        public VehiclesModel Vehicle
        {
            get => _vehicle;
            set => SetProperty(ref _vehicle, value);
        }

        public double? VehicleMotorDeachsierungL
        {
            get => Vehicle?.Motor.DeachsierungL;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.DeachsierungL = value;
                RefreshHubradius();
            }
        }

        public UnitListItem VehicleMotorDeachsierungLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.DeachsierungLUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.DeachsierungLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorDeachsierungL));
            }
        }

        public double? VehicleMotorHubL
        {
            get => Vehicle?.Motor.HubL;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.HubL = value;
                RefreshHubradius();
            }
        }

        public UnitListItem VehicleMotorHubLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.HubLUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.HubLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorHubL));
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
            get => _vehicleMotorHubRUnit;
            set
            {
                VehicleMotorHubR = Core.Helpers.Functions.UpdateValue(VehicleMotorHubR, _vehicleMotorHubRUnit, value);

                SetProperty(ref _vehicleMotorHubRUnit, value);
            }
        }

        public double? VehicleMotorPleulL
        {
            get => Vehicle?.Motor.PleulL;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.PleulL = value;
                RefreshHubradius();
            }
        }

        public UnitListItem VehicleMotorPleulLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.PleulLUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.PleulLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorPleulL));
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