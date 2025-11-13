// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.ViewModels
{
    using CommunityToolkit.Maui;
	using CommunityToolkit.Maui.Views;
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

    public partial class MotorVerdichtungViewModel : ViewModelBase
    {
        private readonly IPopupService _popupService;

        public MotorVerdichtungViewModel(
            ILogger<MotorVerdichtungViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService,
            IPopupService popupService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _popupService = popupService;

            VolumeQuantityUnits = new VolumeQuantity();
            LengthQuantityUnits = new LengthQuantity();

            Vehicle = new VehiclesModel();
            Vehicle.Motor = new MotorModel();
        }

        #region Methods




        /// <summary>
        /// Inserts the data.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.HubraumV.HasValue)
            {
                VehicleMotorHubraumV = helperVehicle.Motor.HubraumV;
                OnPropertyChanged(nameof(VehicleMotorHubraumV));
            }

            if (helperVehicle.Motor.BrennraumV.HasValue)
            {
                VehicleMotorBrennraumV = helperVehicle.Motor.BrennraumV;
                OnPropertyChanged(nameof(VehicleMotorBrennraumV));
            }

            if (helperVehicle.Motor.BohrungD.HasValue)
            {
                VehicleMotorBohrungD = helperVehicle.Motor.BohrungD;
                OnPropertyChanged(nameof(VehicleMotorBohrungD));
            }
        }

        [RelayCommand]
        private async Task ShowHelperVehiclesAsync()
        {
            var result = await _popupService.ShowPopupAsync<VehiclesViewModel>(Shell.Current);
            if (result is VehiclesModel vehicleModel)
            {
                InsertHelperVehicle(vehicleModel);
            }
        }

        /// <summary>
        /// Refreshes the zielverdichtung.
        /// </summary>
        private void Refresh_zielverdichtung()
        {
            if (VehicleMotorHubraumV.HasValue && VehicleMotorBrennraumV.HasValue && VehicleMotorBohrungD.HasValue && Zielverdichtung != 0)
            {
                AbdrehenLength = EngineLogic.GetToDecreasingLength(
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorHubraumV.Value,
                        VehicleMotorHubraumVUnit.UnitEnumValue,
                        MotorModel.HubraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorBrennraumV.Value,
                        VehicleMotorBrennraumVUnit.UnitEnumValue,
                        MotorModel.BrennraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorBohrungD.Value,
                        VehicleMotorBohrungDUnit.UnitEnumValue,
                        MotorModel.BohrungDBaseUnit),
                    Zielverdichtung.Value);
            }
        }

        /// <summary>
        /// Refreshes the verdichtung.
        /// </summary>
        private void RefreshVerdichtung()
        {
            if (VehicleMotorHubraumV.HasValue && VehicleMotorBrennraumV.HasValue && VehicleMotorBohrungD.HasValue)
            {
                DerzeitigeVerdichtung = EngineLogic.GetCompression(
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorHubraumV.Value,
                        VehicleMotorHubraumVUnit.UnitEnumValue,
                        MotorModel.HubraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorBrennraumV.Value,
                        VehicleMotorBrennraumVUnit.UnitEnumValue,
                        MotorModel.BrennraumVBaseUnit),
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorBohrungD.Value,
                        VehicleMotorBohrungDUnit.UnitEnumValue,
                        MotorModel.BohrungDBaseUnit));
            }
        }

        #endregion Methods

        #region Values

        private readonly ILogger<MotorVerdichtungViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private double? _abdrehenLength;
        private UnitListItem _abdrehenLengthUnit;
        private double? _derzeitigeVerdichtung;
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
            get => _abdrehenLength;
            set => SetProperty(ref _abdrehenLength, value);
        }

        /// <summary>
        /// Gets or sets the abdrehen length unit.
        /// </summary>
        /// <value>The abdrehen length unit.</value>
        public UnitListItem AbdrehenLengthUnit
        {
            get => _abdrehenLengthUnit ?? LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(AbdrehenLengthBaseUnit));
            set
            {
                AbdrehenLength = Core.Helpers.Functions.UpdateValue(AbdrehenLength, _abdrehenLengthUnit, value);

                SetProperty(ref _abdrehenLengthUnit, value);
            }
        }

        /// <summary>
        /// Gets or sets the derzeitige verdichtung.
        /// </summary>
        /// <value>The derzeitige verdichtung.</value>
        public double? DerzeitigeVerdichtung
        {
            get => _derzeitigeVerdichtung;
            set => SetProperty(ref _derzeitigeVerdichtung, value);
        }

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
            get => _vehicle;
            set => SetProperty(ref _vehicle, value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d.
        /// </summary>
        /// <value>The vehicle motor bohrung d.</value>
        public double? VehicleMotorBohrungD
        {
            get => Vehicle?.Motor?.BohrungD;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.BohrungD = value;
                RefreshVerdichtung();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d unit.
        /// </summary>
        /// <value>The vehicle motor bohrung d unit.</value>
        public UnitListItem VehicleMotorBohrungDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.BohrungDUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.BohrungDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorBohrungD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v.
        /// </summary>
        /// <value>The vehicle motor brennraum v.</value>
        public double? VehicleMotorBrennraumV
        {
            get => Vehicle?.Motor?.BrennraumV;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.BrennraumV = value;
                RefreshVerdichtung();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h unit.</value>
        public UnitListItem VehicleMotorBrennraumVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.BrennraumVUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.BrennraumVUnit = (UnitsNet.Units.VolumeUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorBrennraumV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v.
        /// </summary>
        /// <value>The vehicle motor hubraum v.</value>
        public double? VehicleMotorHubraumV
        {
            get => Vehicle?.Motor?.HubraumV;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.HubraumV = value;
                RefreshVerdichtung();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v unit.
        /// </summary>
        /// <value>The vehicle motor hubraum v unit.</value>
        public UnitListItem VehicleMotorHubraumVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.HubraumVUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.HubraumVUnit = (UnitsNet.Units.VolumeUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorHubraumV));
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
            get => _zielverdichtung;
            set
            {
                SetProperty(ref _zielverdichtung, value);
                Refresh_zielverdichtung();
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
