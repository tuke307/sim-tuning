// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Models;
using SimTuning.Core.Models.Quantity;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnitsNet.Units;

namespace SimTuning.Maui.UI.ViewModels
{
    public partial class AuslassTheorieViewModel : ViewModelBase
    {
        private readonly IPopupService _popupService;

        public AuslassTheorieViewModel(
            ILogger<AuslassTheorieViewModel> logger,
            IVehicleService vehicleService,
            IPopupService popupService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _popupService = popupService;

            AreaQuantityUnits = new AreaQuantity();
            LengthQuantityUnits = new LengthQuantity();
            SpeedQuantityUnits = new SpeedQuantity();
            TemperatureQuantityUnits = new TemperatureQuantity();

            // Vehicle Creation
            Vehicle = new VehiclesModel();
            Vehicle.Motor = new MotorModel();
            Vehicle.Motor.Auslass = new AuslassModel();
            Vehicle.Motor.Auslass.Auspuff = new AuspuffModel();

            ModAuspuff = new AuspuffModel();

            // andere unit vorbelegen
            VehicleMotorAuslassFlaecheAUnit = AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(AreaUnit.SquareCentimeter));
        }

        #region Methods

        /// <summary>
        /// Inserts the data.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.Auslass.FlaecheA.HasValue)
            {
                VehicleMotorAuslassFlaecheA = helperVehicle.Motor.Auslass.FlaecheA;
            }

            if (helperVehicle.Motor.ResonanzU.HasValue)
            {
                VehicleMotorResonanzU = helperVehicle.Motor.ResonanzU;
            }

            if (helperVehicle.Motor.Auslass.SteuerzeitSZ.HasValue)
            {
                VehicleMotorAuslassSteuerzeitSZ = helperVehicle.Motor.Auslass.SteuerzeitSZ;
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
        /// Refreshes the auspuff geschwindigkeit.
        /// </summary>
        private void Refresh_AuspuffGeschwindigkeit()
        {
            if (ModAuspuffAbgasT.HasValue)
            {
                ModAuspuffAbgasV = AuslassLogic.GetGasGeschwindigkeit(ModAuspuffAbgasT.Value);
                OnPropertyChanged(nameof(ModAuspuffAbgasV));
            }
        }

        /// <summary>
        /// Refreshes the kruemmer d.
        /// </summary>
        private void Refresh_KruemmerD()
        {
            if (VehicleMotorAuslassFlaecheA.HasValue)
            {
                KruemmerSpannneD =
                    AuslassLogic.GetKruemmerDurchmesser(
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorAuslassFlaecheA.Value,
                        VehicleMotorAuslassFlaecheAUnit.UnitEnumValue,
                        AreaUnit.SquareCentimeter), 10)
                    +
                    " - "
                    +
                    AuslassLogic.GetKruemmerDurchmesser(
                    UnitsNet.UnitConverter.Convert(
                        VehicleMotorAuslassFlaecheA.Value,
                        VehicleMotorAuslassFlaecheAUnit.UnitEnumValue,
                        AreaUnit.SquareCentimeter), 20);
            }
        }

        /// <summary>
        /// Refreshes the kruemmer l.
        /// </summary>
        private void Refresh_KruemmerL()
        {
            if (VehicleMotorAuslassAuspuffKruemmerD.HasValue && VehicleMotorAuslassAuspuffKruemmerF.HasValue)
            {
                VehicleMotorAuslassAuspuffKruemmerL = AuslassLogic.GetKruemmerLaenge(
                     UnitsNet.UnitConverter.Convert(
                         VehicleMotorAuslassAuspuffKruemmerD.Value,
                         VehicleMotorAuslassAuspuffKruemmerDUnit.UnitEnumValue,
                         LengthUnit.Millimeter),
                     VehicleMotorAuslassAuspuffKruemmerF.Value,
                     0);

                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffKruemmerL));
            }
        }

        /// <summary>
        /// Refreshes the resonanzlaenge.
        /// </summary>
        private void Refresh_Resonanzlaenge()
        {
            if (VehicleMotorAuslassSteuerzeitSZ.HasValue && VehicleMotorAuslassAuspuffAbgasT.HasValue && VehicleMotorResonanzU.HasValue)
            {
                VehicleMotorAuslassAuspuffResonanzL = AuslassLogic.GetResonanzLaenge(VehicleMotorAuslassSteuerzeitSZ.Value, VehicleMotorAuslassAuspuffAbgasT.Value, VehicleMotorResonanzU.Value);
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffResonanzL));
            }
        }

        #endregion Methods

        #region Values

        private readonly ILogger<AuslassTheorieViewModel> _logger;
        private readonly IVehicleService _vehicleService;

        private string _kruemmerSpannneD;

        private AuspuffModel _modAuspuff;
        private VehiclesModel _vehicle;

        /// <summary>
        /// Gets the area quantity units.
        /// </summary>
        /// <value>The area quantity units.</value>
        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the kruemmer spannne d.
        /// </summary>
        /// <value>The kruemmer spannne d.</value>
        public string KruemmerSpannneD
        {
            get => _kruemmerSpannneD;
            set => SetProperty(ref _kruemmerSpannneD, value);
        }

        /// <summary>
        /// Gets the length quantity units.
        /// </summary>
        /// <value>The length quantity units.</value>
        public ObservableCollection<UnitListItem> LengthQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the mod auspuff.
        /// </summary>
        /// <value>The mod auspuff.</value>
        public AuspuffModel ModAuspuff
        {
            get => _modAuspuff;
            set => SetProperty(ref _modAuspuff, value);
        }

        /// <summary>
        /// Gets or sets the mod abgas t.
        /// </summary>
        /// <value>The mod abgas t.</value>
        public double? ModAuspuffAbgasT
        {
            get => ModAuspuff?.AbgasT;
            set
            {
                if (ModAuspuff == null)
                {
                    return;
                }

                ModAuspuff.AbgasT = value;
                Refresh_AuspuffGeschwindigkeit();
            }
        }

        /// <summary>
        /// Gets or sets the mod auspuff abgas t unit.
        /// </summary>
        /// <value>The mod auspuff abgas t unit.</value>
        public UnitListItem ModAuspuffAbgasTUnit
        {
            get => TemperatureQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(ModAuspuff.AbgasTUnit));
            set
            {
                if (ModAuspuff == null)
                {
                    return;
                }

                ModAuspuff.AbgasTUnit = (UnitsNet.Units.TemperatureUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(ModAuspuffAbgasT));
            }
        }

        /// <summary>
        /// Gets or sets the mod abgas v.
        /// </summary>
        /// <value>The mod abgas v.</value>
        public double? ModAuspuffAbgasV
        {
            get => ModAuspuff?.AbgasV;
            set
            {
                if (ModAuspuff == null)
                {
                    return;
                }

                ModAuspuff.AbgasV = value;
            }
        }

        /// <summary>
        /// Gets or sets the mod abgas v unit.
        /// </summary>
        /// <value>The mod abgas v unit.</value>
        public UnitListItem ModAuspuffAbgasVUnit
        {
            get => SpeedQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(ModAuspuff.AbgasVUnit));
            set
            {
                if (ModAuspuff == null)
                {
                    return;
                }

                ModAuspuff.AbgasVUnit = (UnitsNet.Units.SpeedUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(ModAuspuffAbgasV));
            }
        }

        /// <summary>
        /// Gets the speed quantity units.
        /// </summary>
        /// <value>The speed quantity units.</value>
        public ObservableCollection<UnitListItem> SpeedQuantityUnits { get; }

        /// <summary>
        /// Gets the temperature quantity units.
        /// </summary>
        /// <value>The temperature quantity units.</value>
        public ObservableCollection<UnitListItem> TemperatureQuantityUnits { get; }

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
        /// Gets or sets the vehicle motor auslass auspuff abgas t.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff abgas t.</value>
        public double? VehicleMotorAuslassAuspuffAbgasT
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.AbgasT;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.AbgasT = value;
                Refresh_Resonanzlaenge();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff abgas t unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff abgas t unit.</value>
        public UnitListItem VehicleMotorAuslassAuspuffAbgasTUnit
        {
            get => TemperatureQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.Auspuff?.AbgasTUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.AbgasTUnit = (UnitsNet.Units.TemperatureUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffAbgasT));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer d.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerD
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerD;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.KruemmerD = value;
                Refresh_KruemmerL();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer d unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer d unit.</value>
        public UnitListItem VehicleMotorAuslassAuspuffKruemmerDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerDUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.KruemmerDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffKruemmerD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer f.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer f.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerF
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerF;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.KruemmerF = value;
                Refresh_KruemmerL();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer l.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerL
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerL;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.KruemmerL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer ld unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer ld unit.</value>
        public UnitListItem VehicleMotorAuslassAuspuffKruemmerLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerLUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.KruemmerLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffKruemmerL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff resonanz l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff resonanz l.</value>
        public double? VehicleMotorAuslassAuspuffResonanzL
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.ResonanzL;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.ResonanzL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff resonanz l unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff resonanz l unit.</value>
        public UnitListItem VehicleMotorAuslassAuspuffResonanzLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.Auspuff?.ResonanzLUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.ResonanzLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffResonanzL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a.</value>
        public double? VehicleMotorAuslassFlaecheA
        {
            get => Vehicle?.Motor?.Auslass?.FlaecheA;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                Refresh_KruemmerD();
                Vehicle.Motor.Auslass.FlaecheA = value;
                OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a unit.</value>
        public UnitListItem VehicleMotorAuslassFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.FlaecheAUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.FlaecheAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor auslass steuerzeit sz.</value>
        public double? VehicleMotorAuslassSteuerzeitSZ
        {
            get => Vehicle?.Motor?.Auslass?.SteuerzeitSZ;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                Refresh_Resonanzlaenge();
                Vehicle.Motor.Auslass.SteuerzeitSZ = value;
                OnPropertyChanged(nameof(VehicleMotorAuslassSteuerzeitSZ));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor resonanz u.
        /// </summary>
        /// <value>The vehicle motor resonanz u.</value>
        public double? VehicleMotorResonanzU
        {
            get => Vehicle?.Motor?.ResonanzU;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Refresh_Resonanzlaenge();
                Vehicle.Motor.ResonanzU = value;
                OnPropertyChanged(nameof(VehicleMotorResonanzU));
            }
        }

        #endregion Values
    }
}
