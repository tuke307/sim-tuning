// Copyright (c) 2025 tuke productions. All rights reserved.
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
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SimTuning.Maui.UI.ViewModels
{
    public partial class AuslassAnwendungViewModel : ViewModelBase
    {
        private readonly IPopupService _popupService;

        public AuslassAnwendungViewModel(
            ILogger<AuslassAnwendungViewModel> logger,
            IVehicleService vehicleService,
            IPopupService popupService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _popupService = popupService;

            // Vehicle Creation
            _vehicle = new VehiclesModel();
            _vehicle.Motor = new MotorModel();
            _vehicle.Motor.Auslass = new AuslassModel();
            _vehicle.Motor.Auslass.Auspuff = new AuspuffModel();

            AreaQuantityUnits = new AreaQuantity();
            VolumeQuantityUnits = new VolumeQuantity();
            LengthQuantityUnits = new LengthQuantity();
            SpeedQuantityUnits = new SpeedQuantity();

            DiffStages = new List<string>() { "One Stage", "Two Stage", "Three Stage" };

            // Methods
            CalculateCommand = new RelayCommand(Calculate);
            DiffusorStageCommand = new RelayCommand<int>(DiffusorStage);
        }

        #region Methods

        /// <summary>
        /// Diffusors the stage.
        /// </summary>
        public void DiffusorStage(int stage)
        {
            var auspuff = Vehicle?.Motor?.Auslass?.Auspuff;
            if (auspuff == null)
            {
                return;
            }

            auspuff.DiffusorStage = stage;
        }

        /// <summary>
        /// Fügt Vehicle-Helper ein.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.Auslass.FlaecheA.HasValue)
            {
                VehicleMotorAuslassFlaecheA = helperVehicle.Motor.Auslass.FlaecheA.Value;
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
        /// Berechnet den Auspuff.
        /// </summary>
        /// <returns>Auspuff-Bild als Stream.</returns>
        protected void Calculate()
        {
            if (Vehicle is not VehiclesModel vehicle)
            {
                return;
            }

            Stream stream = SimTuning.Core.Converters.Converts.SKBitmapToStream(AuslassLogic.Auspuff(ref vehicle));
            Vehicle = vehicle;

            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffKruemmerD));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffKruemmerL));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffDiffusorD));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffDiffusorL));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffMittelteilD));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffMittelteilL));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffGegenkonusD));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffGegenkonusL));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffEndrohrL));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffGesamtL));
            OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffResonanzL));

            Auspuff = ImageSource.FromStream(() => stream);
        }

        #endregion Methods

        #region Values

        private readonly ILogger<AuslassAnwendungViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private ImageSource? _auspuff;
        private VehiclesModel? _helperVehicle;

        private VehiclesModel? _vehicle;

        /// <summary>
        /// Gets or sets the area quantity units.
        /// </summary>
        /// <value>The area quantity units.</value>
        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; protected set; }

        public ImageSource? Auspuff
        {
            get => _auspuff;
            private set => SetProperty(ref _auspuff, value);
        }

        /// <summary>
        /// Gets or sets the calculate command.
        /// </summary>
        /// <value>The calculate command.</value>
        public IRelayCommand CalculateCommand { get; set; }

        /// <summary>
        /// Gets the difference stages.
        /// </summary>
        /// <value>The difference stages.</value>
        public List<string> DiffStages { get; private set; }

        /// <summary>
        /// Gets or sets the diffusor stage command.
        /// </summary>
        /// <value>The diffusor stage command.</value>
        public IRelayCommand DiffusorStageCommand { get; set; }

        /// <summary>
        /// Gets or sets the length quantity units.
        /// </summary>
        /// <value>The length quantity units.</value>
        public ObservableCollection<UnitListItem> LengthQuantityUnits { get; protected set; }

        /// <summary>
        /// Gets or sets the speed quantity units.
        /// </summary>
        /// <value>The speed quantity units.</value>
        public ObservableCollection<UnitListItem> SpeedQuantityUnits { get; protected set; }

        /// <summary>
        /// Gets or sets the vehicle.
        /// </summary>
        /// <value>The vehicle.</value>
        public VehiclesModel? Vehicle
        {
            get => _vehicle;
            set => SetProperty(ref _vehicle, value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff abgas v.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff abgas v.</value>
        public double? VehicleMotorAuslassAuspuffAbgasV
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.AbgasV;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.AbgasV = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff abgas v unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff abgas v unit.</value>
        public UnitListItem? VehicleMotorAuslassAuspuffAbgasVUnit
        {
            get => SpeedQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.Auspuff?.AbgasVUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null || value == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.AbgasVUnit = (UnitsNet.Units.SpeedUnit)value.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffAbgasV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor d.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorD
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorD;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.DiffusorD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor l.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorL
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorL;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.DiffusorL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor w1.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor w1.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorW1
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorW1;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.DiffusorW1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor w2.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor w2.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorW2
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorW2;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.DiffusorW2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor w3.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor w3.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorW3
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorW3;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.DiffusorW3 = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr d.</value>
        public double? VehicleMotorAuslassAuspuffEndrohrD
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrD;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.EndrohrD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr d unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr d unit.</value>
        public UnitListItem? VehicleMotorAuslassAuspuffEndrohrDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrDUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null || value == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.EndrohrDUnit = (UnitsNet.Units.LengthUnit)value.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffEndrohrD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr l.</value>
        public double? VehicleMotorAuslassAuspuffEndrohrL
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrL;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.EndrohrL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr l unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr l unit.</value>
        public UnitListItem? VehicleMotorAuslassAuspuffEndrohrLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrLUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null || value == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.EndrohrLUnit = (UnitsNet.Units.LengthUnit)value.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassAuspuffEndrohrL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gegenkonus d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gegenkonus d.</value>
        public double? VehicleMotorAuslassAuspuffGegenkonusD
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.GegenkonusD;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.GegenkonusD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gegenkonus l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gegenkonus l.</value>
        public double? VehicleMotorAuslassAuspuffGegenkonusL
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.GegenkonusL;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.GegenkonusL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gegen konus w.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gegen konus w.</value>
        public double? VehicleMotorAuslassAuspuffGegenKonusW
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.GegenKonusW;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.GegenKonusW = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gesamt l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gesamt l.</value>
        public double? VehicleMotorAuslassAuspuffGesamtL
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.GesamtL;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.GesamtL = value;
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
        /// Gets or sets the vehicle motor auslass auspuff kruemmer w.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer w.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerW
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerW;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.KruemmerW = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff mittelteil d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff mittelteil d.</value>
        public double? VehicleMotorAuslassAuspuffMittelteilD
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.MittelteilD;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.MittelteilD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff mittelteil f.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff mittelteil f.</value>
        public double? VehicleMotorAuslassAuspuffMittelteilF
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.MittelteilF;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.MittelteilF = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff mittelteil l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff mittelteil l.</value>
        public double? VehicleMotorAuslassAuspuffMittelteilL
        {
            get => Vehicle?.Motor?.Auslass?.Auspuff?.MittelteilL;
            set
            {
                if (Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.Auspuff.MittelteilL = value;
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
        /// Gets or sets the vehicle motor auslass durchmesser d.
        /// </summary>
        /// <value>The vehicle motor auslass durchmesser d.</value>
        public double? VehicleMotorAuslassDurchmesserD
        {
            get => Vehicle?.Motor?.Auslass?.DurchmesserD;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.DurchmesserD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass durchmesser d unit.
        /// </summary>
        /// <value>The vehicle motor auslass durchmesser d unit.</value>
        public UnitListItem? VehicleMotorAuslassDurchmesserDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.DurchmesserDUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null || value == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.DurchmesserDUnit = (UnitsNet.Units.LengthUnit)value.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassDurchmesserD));
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

                Vehicle.Motor.Auslass.FlaecheA = value;
                OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a unit.</value>
        public UnitListItem? VehicleMotorAuslassFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.FlaecheAUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null || value == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.FlaecheAUnit = (UnitsNet.Units.AreaUnit)value.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l.</value>
        public double? VehicleMotorAuslassLaengeL
        {
            get => Vehicle?.Motor?.Auslass?.LaengeL;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.LaengeL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l unit.</value>
        public UnitListItem? VehicleMotorAuslassLaengeLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.LaengeLUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null || value == null)
                {
                    return;
                }

                Vehicle.Motor.Auslass.LaengeLUnit = (UnitsNet.Units.LengthUnit)value.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassLaengeL));
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

                Vehicle.Motor.ResonanzU = value;
                OnPropertyChanged(nameof(VehicleMotorResonanzU));
            }
        }

        /// <summary>
        /// Gets or sets the volume quantity units.
        /// </summary>
        /// <value>The volume quantity units.</value>
        public ObservableCollection<UnitListItem> VolumeQuantityUnits { get; protected set; }

        #endregion Values
    }
}
