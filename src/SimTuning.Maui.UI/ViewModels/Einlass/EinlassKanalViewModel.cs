// Copyright (c) 2021 tuke productions. All rights reserved.
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
            this._logger = logger;
            this._vehicleService = vehicleService;

            this.AreaQuantityUnits = new AreaQuantity();
            this.VolumeQuantityUnits = new VolumeQuantity();
            this.LengthQuantityUnits = new LengthQuantity();

            // Vehicle Creation
            this.Vehicle = new VehiclesModel();
            this.Vehicle.Motor = new MotorModel();
            this.Vehicle.Motor.Einlass = new EinlassModel();

            this.ResonanzlaengeUnit = this.LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
        }

        #region Methods

        /// <summary>
        /// Inserts the data.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.Einlass.FlaecheA.HasValue)
            {
                this.VehicleMotorEinlassFlaecheA = helperVehicle.Motor.Einlass.FlaecheA;
            }

            if (helperVehicle.Motor.Einlass.SteuerzeitSZ.HasValue)
            {
                this.Einlasssteuerwinkel = helperVehicle.Motor.Einlass.SteuerzeitSZ;
            }

            if (helperVehicle.Motor.ResonanzU.HasValue)
            {
                this.VehicleMotorResonanzU = helperVehicle.Motor.ResonanzU;
            }

            if (helperVehicle.Motor.KurbelgehaeuseV.HasValue)
            {
                this.VehicleMotorKurbelgehaeuseV = helperVehicle.Motor.KurbelgehaeuseV;
            }

            if (helperVehicle.Motor.Einlass.DurchmesserD.HasValue)
            {
                this.VehicleMotorEinlassDurchmesserD = helperVehicle.Motor.Einlass.DurchmesserD;
            }
        }

        private void RefreshResonanzlaenge()
        {
            if (this.VehicleMotorEinlassFlaecheA.HasValue && this.Einlasssteuerwinkel.HasValue && this.VehicleMotorKurbelgehaeuseV.HasValue && this.VehicleMotorResonanzU.HasValue && this.VehicleMotorEinlassDurchmesserD.HasValue)
            {
                this.Resonanzlaenge = EinlassLogic.GetResonanzLaenge(
                    UnitsNet.UnitConverter.Convert(this.VehicleMotorEinlassFlaecheA.Value, this.VehicleMotorEinlassFlaecheAUnit.UnitEnumValue, AreaUnit.SquareCentimeter),
                    this.Einlasssteuerwinkel.Value,
                    UnitsNet.UnitConverter.Convert(this.VehicleMotorKurbelgehaeuseV.Value, this.VehicleMotorKurbelgehaeuseVUnit.UnitEnumValue, VolumeUnit.CubicCentimeter),
                    this.VehicleMotorResonanzU.Value,
                    UnitsNet.UnitConverter.Convert(this.VehicleMotorEinlassDurchmesserD.Value, this.VehicleMotorEinlassDurchmesserDUnit.UnitEnumValue, LengthUnit.Centimeter));
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
                this.OnPropertyChanged(nameof(this.Einlasssteuerwinkel));
                RefreshResonanzlaenge();
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
            get => this._vehicle;
            set => this.SetProperty(ref this._vehicle, value);
        }

        public double? VehicleMotorEinlassDurchmesserD
        {
            get => this.Vehicle?.Motor?.Einlass?.DurchmesserD;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Einlass.DurchmesserD = value;
                this.OnPropertyChanged(nameof(this.VehicleMotorEinlassDurchmesserD));
                RefreshResonanzlaenge();
            }
        }

        public UnitListItem VehicleMotorEinlassDurchmesserDUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.DurchmesserDUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Einlass.DurchmesserDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorEinlassDurchmesserD));
            }
        }

        public double? VehicleMotorEinlassFlaecheA
        {
            get => this.Vehicle?.Motor?.Einlass?.FlaecheA;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Einlass.FlaecheA = value;
                this.OnPropertyChanged(nameof(this.VehicleMotorEinlassFlaecheA));
                RefreshResonanzlaenge();
            }
        }

        public UnitListItem VehicleMotorEinlassFlaecheAUnit
        {
            get => this.AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.FlaecheAUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Einlass.FlaecheAUnit = (UnitsNet.Units.AreaUnit?)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorEinlassFlaecheA));
            }
        }

        public double? VehicleMotorKurbelgehaeuseV
        {
            get => this.Vehicle?.Motor?.KurbelgehaeuseV;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }

                this.Vehicle.Motor.KurbelgehaeuseV = value;
                this.OnPropertyChanged(nameof(this.VehicleMotorKurbelgehaeuseV));
                RefreshResonanzlaenge();
            }
        }

        public UnitListItem VehicleMotorKurbelgehaeuseVUnit
        {
            get => this.VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.KurbelgehaeuseVUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.KurbelgehaeuseVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorKurbelgehaeuseV));
            }
        }

        public double? VehicleMotorResonanzU
        {
            get => this.Vehicle?.Motor?.ResonanzU;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.ResonanzU = value;
                this.OnPropertyChanged(nameof(this.VehicleMotorResonanzU));
                RefreshResonanzlaenge();
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