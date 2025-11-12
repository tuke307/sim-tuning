// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Models;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using System.Collections.ObjectModel;
using UnitsNet.Units;

namespace SimTuning.Maui.UI.ViewModels
{
    public class EinlassKanalViewModel : ViewModelBase
    {
        public EinlassKanalViewModel(
            ILogger<EinlassKanalViewModel> logger,
            IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;

            AreaQuantityUnits = new AreaQuantity();
            VolumeQuantityUnits = new VolumeQuantity();
            LengthQuantityUnits = new LengthQuantity();

            // Vehicle Creation
            Vehicle = new VehiclesModel();
            Vehicle.Motor = new MotorModel();
            Vehicle.Motor.Einlass = new EinlassModel();

            ResonanzlaengeUnit = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
        }

        #region Methods

        /// <summary>
        /// Inserts the data.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.Einlass.FlaecheA.HasValue)
            {
                VehicleMotorEinlassFlaecheA = helperVehicle.Motor.Einlass.FlaecheA;
            }

            if (helperVehicle.Motor.Einlass.SteuerzeitSZ.HasValue)
            {
                Einlasssteuerwinkel = helperVehicle.Motor.Einlass.SteuerzeitSZ;
            }

            if (helperVehicle.Motor.ResonanzU.HasValue)
            {
                VehicleMotorResonanzU = helperVehicle.Motor.ResonanzU;
            }

            if (helperVehicle.Motor.KurbelgehaeuseV.HasValue)
            {
                VehicleMotorKurbelgehaeuseV = helperVehicle.Motor.KurbelgehaeuseV;
            }

            if (helperVehicle.Motor.Einlass.DurchmesserD.HasValue)
            {
                VehicleMotorEinlassDurchmesserD = helperVehicle.Motor.Einlass.DurchmesserD;
            }
        }

        private void CalculateResonanzlaenge()
        {
            if (VehicleMotorEinlassFlaecheA.HasValue &&
                Einlasssteuerwinkel.HasValue &&
                VehicleMotorKurbelgehaeuseV.HasValue &&
                VehicleMotorResonanzU.HasValue &&
                VehicleMotorEinlassDurchmesserD.HasValue)
            {
                Resonanzlaenge = EinlassLogic.GetResonanzLaenge(
                    UnitsNet.UnitConverter.Convert(VehicleMotorEinlassFlaecheA.Value, VehicleMotorEinlassFlaecheAUnit.UnitEnumValue, AreaUnit.SquareCentimeter),
                    Einlasssteuerwinkel.Value,
                    UnitsNet.UnitConverter.Convert(VehicleMotorKurbelgehaeuseV.Value, VehicleMotorKurbelgehaeuseVUnit.UnitEnumValue, VolumeUnit.CubicCentimeter),
                    VehicleMotorResonanzU.Value,
                    UnitsNet.UnitConverter.Convert(VehicleMotorEinlassDurchmesserD.Value, VehicleMotorEinlassDurchmesserDUnit.UnitEnumValue, LengthUnit.Centimeter));
            }
        }

        #endregion Methods

        #region Values

        private readonly ILogger<EinlassKanalViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private double? _einlasssteuerwinkel;

        private double? _resonanzlaenge;

        private UnitListItem _resonanzlaengeUnit;

        private VehiclesModel _vehicle;

        /// <summary>
        /// Gets the area quantity units.
        /// </summary>
        /// <value>The area quantity units.</value>
        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; }

        public double? Einlasssteuerwinkel
        {
            get => _einlasssteuerwinkel;
            set
            {
                SetProperty(ref _einlasssteuerwinkel, value);
                CalculateResonanzlaenge();
            }
        }

        /// <summary>
        /// Gets the length quantity units.
        /// </summary>
        /// <value>The length quantity units.</value>
        public ObservableCollection<UnitListItem> LengthQuantityUnits { get; }

        public double? Resonanzlaenge
        {
            get => _resonanzlaenge;
            set { SetProperty(ref _resonanzlaenge, value); }
        }

        public UnitListItem ResonanzlaengeUnit
        {
            get => _resonanzlaengeUnit;
            set
            {
                Resonanzlaenge = Core.Helpers.Functions.UpdateValue(Resonanzlaenge, _resonanzlaengeUnit, value);

                SetProperty(ref _resonanzlaengeUnit, value);
            }
        }

        /// <summary>
        /// Gets or sets the vehicle.
        /// </summary>
        /// <value>The vehicle.</value>
        public VehiclesModel Vehicle
        {
            get => _vehicle;
            set => SetProperty(ref _vehicle, value);
        }

        public double? VehicleMotorEinlassDurchmesserD
        {
            get => Vehicle?.Motor?.Einlass?.DurchmesserD;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                Vehicle.Motor.Einlass.DurchmesserD = value;
                OnPropertyChanged(nameof(VehicleMotorEinlassDurchmesserD));
                CalculateResonanzlaenge();
            }
        }

        public UnitListItem VehicleMotorEinlassDurchmesserDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.DurchmesserDUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                Vehicle.Motor.Einlass.DurchmesserDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassDurchmesserD));
            }
        }

        public double? VehicleMotorEinlassFlaecheA
        {
            get => Vehicle?.Motor?.Einlass?.FlaecheA;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                Vehicle.Motor.Einlass.FlaecheA = value;
                OnPropertyChanged(nameof(VehicleMotorEinlassFlaecheA));
                CalculateResonanzlaenge();
            }
        }

        public UnitListItem VehicleMotorEinlassFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.FlaecheAUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                Vehicle.Motor.Einlass.FlaecheAUnit = (UnitsNet.Units.AreaUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassFlaecheA));
            }
        }

        public double? VehicleMotorKurbelgehaeuseV
        {
            get => Vehicle?.Motor?.KurbelgehaeuseV;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                Vehicle.Motor.KurbelgehaeuseV = value;
                OnPropertyChanged(nameof(VehicleMotorKurbelgehaeuseV));
                CalculateResonanzlaenge();
            }
        }

        public UnitListItem VehicleMotorKurbelgehaeuseVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.KurbelgehaeuseVUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.KurbelgehaeuseVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorKurbelgehaeuseV));
            }
        }

        public double? VehicleMotorResonanzU
        {
            get => Vehicle?.Motor?.ResonanzU;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.ResonanzU = value;
                OnPropertyChanged(nameof(VehicleMotorResonanzU));
                CalculateResonanzlaenge();
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