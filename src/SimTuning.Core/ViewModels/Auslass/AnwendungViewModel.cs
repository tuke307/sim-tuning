// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SimTuning.Core.Models;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace SimTuning.Core.ViewModels.Auslass
{
    /// <summary>
    /// Auslass-Anwendung-ViewModel.
    /// </summary>

    public class AnwendungViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnwendungViewModel" /> class.
        /// </summary>
        /// <param name="logger">
        /// <inheritdoc cref="ILogger" path="/summary/node()" />
        /// </param>
        /// <param name="INavigationService">
        /// <inheritdoc cref="INavigationService" path="/summary/node()" />
        /// </param>
        public AnwendungViewModel(
            ILogger<AnwendungViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._navigationService = navigationService ;
            this._vehicleService = vehicleService;

            // Vehicle Creation
            this.Vehicle = new VehiclesModel();
            this.Vehicle.Motor = new MotorModel();
            this.Vehicle.Motor.Auslass = new AuslassModel();
            this.Vehicle.Motor.Auslass.Auspuff = new AuspuffModel();

            this.AreaQuantityUnits = new AreaQuantity();
            this.VolumeQuantityUnits = new VolumeQuantity();
            this.LengthQuantityUnits = new LengthQuantity();
            this.SpeedQuantityUnits = new SpeedQuantity();

            this.HelperVehicles = new ObservableCollection<VehiclesModel>(_vehicleService.RetrieveVehicles());

            this.DiffStages = new List<string>() { "One Stage", "Two Stage", "Three Stage" };

            // Methods
            this.CalculateCommand = new RelayCommand(this.Calculate);
            this.InsertDataCommand = new RelayCommand(this.InsertData);
            this.DiffusorStageCommand = new RelayCommand<int>(this.DiffusorStage);
        }

        #region Methods

        /// <summary>
        /// Diffusors the stage.
        /// </summary>
        public void DiffusorStage(int stage)
        {
            this.Vehicle.Motor.Auslass.Auspuff.DiffusorStage = stage;
        }

        /// <summary>
        /// Fügt Vehicle-Helper ein.
        /// </summary>
        public void InsertData()
        {
            if (this.HelperVehicle.Motor.Auslass.FlaecheA.HasValue)
            {
                this.VehicleMotorAuslassFlaecheA = this.HelperVehicle.Motor.Auslass.FlaecheA.Value;
            }

            if (this.HelperVehicle.Motor.ResonanzU.HasValue)
            {
                this.VehicleMotorResonanzU = this.HelperVehicle.Motor.ResonanzU;
            }

            if (this.HelperVehicle.Motor.Auslass.SteuerzeitSZ.HasValue)
            {
                this.VehicleMotorAuslassSteuerzeitSZ = this.HelperVehicle.Motor.Auslass.SteuerzeitSZ;
            }

            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassFlaecheA));
            this.OnPropertyChanged(nameof(this.VehicleMotorResonanzU));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassSteuerzeitSZ));
        }


        /// <summary>
        /// Berechnet den Auspuff.
        /// </summary>
        /// <returns>Auspuff-Bild als Stream.</returns>
        protected void Calculate()
        {
            VehiclesModel vehicle = Vehicle;
            Stream stream = SimTuning.Core.Converters.Converts.SKBitmapToStream(AuslassLogic.Auspuff(ref vehicle));
            this.Vehicle = vehicle;

            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffKruemmerD));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffKruemmerL));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffDiffusorD));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffDiffusorL));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffMittelteilD));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffMittelteilL));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffGegenkonusD));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffGegenkonusL));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffEndrohrL));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffGesamtL));
            this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffResonanzL));

            Auspuff = ImageSource.FromStream(() => stream);
        }

        #endregion Methods

        #region Values

       

        protected readonly INavigationService _navigationService;
        private readonly ILogger<AnwendungViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private VehiclesModel _helperVehicle;

        private ObservableCollection<VehiclesModel> _helperVehicles;

        private VehiclesModel _vehicle;
        private ImageSource _auspuff;

        public ImageSource Auspuff
        {
            get => _auspuff;
            private set => SetProperty(ref _auspuff, value);
        }

        /// <summary>
        /// Gets or sets the area quantity units.
        /// </summary>
        /// <value>The area quantity units.</value>
        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; protected set; }

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
        /// Gets or sets the helper vehicle.
        /// </summary>
        /// <value>The helper vehicle.</value>
        public VehiclesModel HelperVehicle
        {
            get => this._helperVehicle;
            set { this.SetProperty(ref this._helperVehicle, value); }
        }

        /// <summary>
        /// Gets or sets the helper vehicles.
        /// </summary>
        /// <value>The helper vehicles.</value>
        public ObservableCollection<VehiclesModel> HelperVehicles
        {
            get => this._helperVehicles;
            set { this.SetProperty(ref this._helperVehicles, value); }
        }

        /// <summary>
        /// Gets or sets the insert data command.
        /// </summary>
        /// <value>The insert data command.</value>
        public IRelayCommand InsertDataCommand { get; set; }

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
        public VehiclesModel Vehicle
        {
            get => this._vehicle;
            set => this.SetProperty(ref this._vehicle, value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff abgas v.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff abgas v.</value>
        public double? VehicleMotorAuslassAuspuffAbgasV
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.AbgasV;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.AbgasV = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff abgas v unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff abgas v unit.</value>
        public UnitListItem VehicleMotorAuslassAuspuffAbgasVUnit
        {
            get => this.SpeedQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.Auspuff?.AbgasVUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.AbgasVUnit = (UnitsNet.Units.SpeedUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffAbgasV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor d.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorD
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorD;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.DiffusorD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor l.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorL
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.DiffusorL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor w1.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor w1.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorW1
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorW1;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.DiffusorW1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor w2.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor w2.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorW2
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorW2;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.DiffusorW2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff diffusor w3.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff diffusor w3.</value>
        public double? VehicleMotorAuslassAuspuffDiffusorW3
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.DiffusorW3;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.DiffusorW3 = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr d.</value>
        public double? VehicleMotorAuslassAuspuffEndrohrD
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrD;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.EndrohrD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr d unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr d unit.</value>
        public UnitListItem VehicleMotorAuslassAuspuffEndrohrDUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrDUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.EndrohrDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffEndrohrD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr l.</value>
        public double? VehicleMotorAuslassAuspuffEndrohrL
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.EndrohrL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff endrohr l unit.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff endrohr l unit.</value>
        public UnitListItem VehicleMotorAuslassAuspuffEndrohrLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.Auspuff?.EndrohrLUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.EndrohrLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorAuslassAuspuffEndrohrL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gegenkonus d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gegenkonus d.</value>
        public double? VehicleMotorAuslassAuspuffGegenkonusD
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.GegenkonusD;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.GegenkonusD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gegenkonus l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gegenkonus l.</value>
        public double? VehicleMotorAuslassAuspuffGegenkonusL
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.GegenkonusL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.GegenkonusL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gegen konus w.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gegen konus w.</value>
        public double? VehicleMotorAuslassAuspuffGegenKonusW
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.GegenKonusW;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.GegenKonusW = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff gesamt l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff gesamt l.</value>
        public double? VehicleMotorAuslassAuspuffGesamtL
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.GesamtL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.GesamtL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer d.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerD
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerD;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.KruemmerD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer f.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer f.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerF
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerF;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.KruemmerF = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer l.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerL
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.KruemmerL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff kruemmer w.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff kruemmer w.</value>
        public double? VehicleMotorAuslassAuspuffKruemmerW
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.KruemmerW;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.KruemmerW = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff mittelteil d.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff mittelteil d.</value>
        public double? VehicleMotorAuslassAuspuffMittelteilD
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.MittelteilD;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.MittelteilD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff mittelteil f.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff mittelteil f.</value>
        public double? VehicleMotorAuslassAuspuffMittelteilF
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.MittelteilF;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.MittelteilF = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff mittelteil l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff mittelteil l.</value>
        public double? VehicleMotorAuslassAuspuffMittelteilL
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.MittelteilL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.MittelteilL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass auspuff resonanz l.
        /// </summary>
        /// <value>The vehicle motor auslass auspuff resonanz l.</value>
        public double? VehicleMotorAuslassAuspuffResonanzL
        {
            get => this.Vehicle?.Motor?.Auslass?.Auspuff?.ResonanzL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass?.Auspuff == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.Auspuff.ResonanzL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass durchmesser d.
        /// </summary>
        /// <value>The vehicle motor auslass durchmesser d.</value>
        public double? VehicleMotorAuslassDurchmesserD
        {
            get => this.Vehicle?.Motor?.Auslass?.DurchmesserD;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.DurchmesserD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass durchmesser d unit.
        /// </summary>
        /// <value>The vehicle motor auslass durchmesser d unit.</value>
        public UnitListItem VehicleMotorAuslassDurchmesserDUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.DurchmesserDUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.DurchmesserDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorAuslassDurchmesserD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a.</value>
        public double? VehicleMotorAuslassFlaecheA
        {
            get => this.Vehicle?.Motor?.Auslass?.FlaecheA;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.FlaecheA = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a unit.</value>
        public UnitListItem VehicleMotorAuslassFlaecheAUnit
        {
            get => this.AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.FlaecheAUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.FlaecheAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorAuslassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l.</value>
        public double? VehicleMotorAuslassLaengeL
        {
            get => this.Vehicle?.Motor?.Auslass?.LaengeL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.LaengeL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l unit.</value>
        public UnitListItem VehicleMotorAuslassLaengeLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.LaengeLUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.LaengeLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorAuslassLaengeL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor auslass steuerzeit sz.</value>
        public double? VehicleMotorAuslassSteuerzeitSZ
        {
            get => this.Vehicle?.Motor?.Auslass?.SteuerzeitSZ;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }

                this.Vehicle.Motor.Auslass.SteuerzeitSZ = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor resonanz u.
        /// </summary>
        /// <value>The vehicle motor resonanz u.</value>
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