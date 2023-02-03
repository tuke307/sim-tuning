// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.ViewModels.Motor
{
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Extensions.Logging;
    using SimTuning.Core;
    using SimTuning.Core.Models;
    using SimTuning.Core.ModuleLogic;
    using SimTuning.Core.Services;
    using SimTuning.Data.Models;
    using SimTuning.Maui.UI.Services;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UnitsNet.Units;

    public class VerdichtungViewModel : ViewModelBase
    {
        public VerdichtungViewModel(
            ILogger<VerdichtungViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._vehicleService = vehicleService;

            this.VolumeQuantityUnits = new VolumeQuantity();
            this.LengthQuantityUnits = new LengthQuantity();

            this.Vehicle = new VehiclesModel();
            this.Vehicle.Motor = new MotorModel();

            this.HelperVehicles = new ObservableCollection<VehiclesModel>(_vehicleService.RetrieveVehicles());

            this.InsertDataCommand = new RelayCommand(this.InsertData);
        }

        #region Methods




        /// <summary>
        /// Inserts the data.
        /// </summary>
        private void InsertData()
        {
            if (this.HelperVehicle.Motor.HubraumV.HasValue)
            {
                this.VehicleMotorHubraumV = this.HelperVehicle.Motor.HubraumV;
                this.OnPropertyChanged(nameof(this.VehicleMotorHubraumV));
            }

            if (this.HelperVehicle.Motor.BrennraumV.HasValue)
            {
                this.VehicleMotorBrennraumV = this.HelperVehicle.Motor.BrennraumV;
                this.OnPropertyChanged(nameof(this.VehicleMotorBrennraumV));
            }

            if (this.HelperVehicle.Motor.BohrungD.HasValue)
            {
                this.VehicleMotorBohrungD = this.HelperVehicle.Motor.BohrungD;
                this.OnPropertyChanged(nameof(this.VehicleMotorBohrungD));
            }
        }

        /// <summary>
        /// Refreshes the zielverdichtung.
        /// </summary>
        private void Refresh_zielverdichtung()
        {
            if (this.VehicleMotorHubraumV.HasValue && this.VehicleMotorBrennraumV.HasValue && this.VehicleMotorBohrungD.HasValue && this.Zielverdichtung != 0)
            {
                this.AbdrehenLength = EngineLogic.GetToDecreasingLength(
                    UnitsNet.UnitConverter.Convert(
                        this.VehicleMotorHubraumV.Value,
                        this.VehicleMotorHubraumVUnit.UnitEnumValue,
                        MotorModel.HubraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        this.VehicleMotorBrennraumV.Value,
                        this.VehicleMotorBrennraumVUnit.UnitEnumValue,
                        MotorModel.BrennraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        this.VehicleMotorBohrungD.Value,
                        this.VehicleMotorBohrungDUnit.UnitEnumValue,
                        MotorModel.BohrungDBaseUnit),
                    this.Zielverdichtung.Value);
            }
        }

        /// <summary>
        /// Refreshes the verdichtung.
        /// </summary>
        private void RefreshVerdichtung()
        {
            if (this.VehicleMotorHubraumV.HasValue && this.VehicleMotorBrennraumV.HasValue && this.VehicleMotorBohrungD.HasValue)
            {
                this.DerzeitigeVerdichtung = EngineLogic.GetCompression(
                    UnitsNet.UnitConverter.Convert(
                        this.VehicleMotorHubraumV.Value,
                        this.VehicleMotorHubraumVUnit.UnitEnumValue,
                        MotorModel.HubraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        this.VehicleMotorBrennraumV.Value,
                        this.VehicleMotorBrennraumVUnit.UnitEnumValue,
                        MotorModel.BrennraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        this.VehicleMotorBohrungD.Value,
                        this.VehicleMotorBohrungDUnit.UnitEnumValue,
                        MotorModel.BohrungDBaseUnit));
            }
        }

        #endregion Methods

        #region Values

        private readonly ILogger<VerdichtungViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private double? _abdrehenLength;
        private UnitListItem _abdrehenLengthUnit;
        private double? _derzeitigeVerdichtung;
        private VehiclesModel _helperVehicle;
        private ObservableCollection<VehiclesModel> _helperVehicles;
        private VehiclesModel _vehicle;
        private double? _zielverdichtung;

        /// <summary>
        /// Gets the abdrehen length base unit.
        /// </summary>
        /// <value>The abdrehen length base unit.</value>
        public static LengthUnit AbdrehenLengthBaseUnit { get => LengthUnit.Millimeter; }

        /// <summary>
        /// Gets or sets the length of the abdrehen.
        /// </summary>
        /// <value>The length of the abdrehen.</value>
        public double? AbdrehenLength
        {
            get => this._abdrehenLength;
            set => this.SetProperty(ref this._abdrehenLength, value);
        }

        /// <summary>
        /// Gets or sets the abdrehen length unit.
        /// </summary>
        /// <value>The abdrehen length unit.</value>
        public UnitListItem AbdrehenLengthUnit
        {
            get => this._abdrehenLengthUnit ?? this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(AbdrehenLengthBaseUnit));
            set
            {
                this.AbdrehenLength = Core.Helpers.Functions.UpdateValue(this.AbdrehenLength, this._abdrehenLengthUnit, value);

                this.SetProperty(ref this._abdrehenLengthUnit, value);
            }
        }

        /// <summary>
        /// Gets or sets the derzeitige verdichtung.
        /// </summary>
        /// <value>The derzeitige verdichtung.</value>
        public double? DerzeitigeVerdichtung
        {
            get => this._derzeitigeVerdichtung;
            set => this.SetProperty(ref this._derzeitigeVerdichtung, value);
        }

        /// <summary>
        /// Gets or sets the helper vehicle.
        /// </summary>
        /// <value>The helper vehicle.</value>
        public VehiclesModel HelperVehicle
        {
            get => this._helperVehicle;
            set => this.SetProperty(ref this._helperVehicle, value);
        }

        /// <summary>
        /// Gets or sets the helper vehicles.
        /// </summary>
        /// <value>The helper vehicles.</value>
        public ObservableCollection<VehiclesModel> HelperVehicles
        {
            get => this._helperVehicles;
            set => this.SetProperty(ref this._helperVehicles, value);
        }

        /// <summary>
        /// Gets or sets the insert data command.
        /// </summary>
        /// <value>The insert data command.</value>
        public IRelayCommand InsertDataCommand { get; set; }

        /// <summary>
        /// Gets the length quantity units.
        /// </summary>
        /// <value>The length quantity units.</value>
        public ObservableCollection<UnitListItem> LengthQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the vehicle.
        /// </summary>
        /// <value>The vehicle.</value>
        public VehiclesModel Vehicle
        {
            get => this._vehicle;
            set => this.SetProperty(ref this._vehicle, value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d.
        /// </summary>
        /// <value>The vehicle motor bohrung d.</value>
        public double? VehicleMotorBohrungD
        {
            get => this.Vehicle?.Motor?.BohrungD;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.BohrungD = value;
                this.RefreshVerdichtung();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d unit.
        /// </summary>
        /// <value>The vehicle motor bohrung d unit.</value>
        public UnitListItem VehicleMotorBohrungDUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.BohrungDUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.BohrungDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorBohrungD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v.
        /// </summary>
        /// <value>The vehicle motor brennraum v.</value>
        public double? VehicleMotorBrennraumV
        {
            get => this.Vehicle?.Motor?.BrennraumV;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.BrennraumV = value;
                this.RefreshVerdichtung();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h unit.</value>
        public UnitListItem VehicleMotorBrennraumVUnit
        {
            get => this.VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.BrennraumVUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.BrennraumVUnit = (UnitsNet.Units.VolumeUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorBrennraumV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v.
        /// </summary>
        /// <value>The vehicle motor hubraum v.</value>
        public double? VehicleMotorHubraumV
        {
            get => this.Vehicle?.Motor?.HubraumV;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.HubraumV = value;
                this.RefreshVerdichtung();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v unit.
        /// </summary>
        /// <value>The vehicle motor hubraum v unit.</value>
        public UnitListItem VehicleMotorHubraumVUnit
        {
            get => this.VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.HubraumVUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.HubraumVUnit = (UnitsNet.Units.VolumeUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorHubraumV));
            }
        }

        /// <summary>
        /// Gets the volume quantity units.
        /// </summary>
        /// <value>The volume quantity units.</value>
        public ObservableCollection<UnitListItem> VolumeQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the zielverdichtung.
        /// </summary>
        /// <value>The zielverdichtung.</value>
        public double? Zielverdichtung
        {
            get => this._zielverdichtung;
            set
            {
                this.SetProperty(ref this._zielverdichtung, value);
                this.Refresh_zielverdichtung();
            }
        }

        /// <summary>
        /// Gets or sets the zielverdichtungen.
        /// </summary>
        /// <value>The zielverdichtungen.</value>
        public List<double?> Zielverdichtungen
        {
            get => new List<double?>() { 8, 8.5, 9, 9.5, 10, 10.5, 11, 11.5, 12 };
        }

        #endregion Values
    }
}