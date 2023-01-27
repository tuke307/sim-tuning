// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ViewModels.Motor
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using SimTuning.Core.ModuleLogic;
    using SimTuning.Core.Services;
    using SimTuning.Data;
    using SimTuning.Data.Models;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;

    public class SteuerdiagrammViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SteuerdiagrammViewModel" /> class.
        /// </summary>
        /// <param name="logger"><inheritdoc cref="ILogger" path="/summary/node()" /></param>
        /// <param name="INavigationService"><inheritdoc cref="INavigationService" path="/summary/node()" /></param>
        /// <param name="vehicleService"><inheritdoc cref="IVehicleService" path="/summary/node()" /></param>
        public SteuerdiagrammViewModel(
            ILogger<SteuerdiagrammViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._vehicleService = vehicleService;

            // lokale liste kreieren
            this.MotorSteuerzeiten = new ObservableCollection<MotorModel>()
            {
                new MotorModel()
                {
                    Name = "sehr kurz",
                    Einlass = new EinlassModel() { SteuerzeitSZ = 100 },
                    Auslass = new AuslassModel() { SteuerzeitSZ = 125 },
                    Ueberstroemer = new UeberstroemerModel() { SteuerzeitSZ = 100 },
                },
                new MotorModel()
                {
                    Name = "kurz",
                    Einlass = new EinlassModel() { SteuerzeitSZ = 120 },
                    Auslass = new AuslassModel() { SteuerzeitSZ = 145 },
                    Ueberstroemer = new UeberstroemerModel() { SteuerzeitSZ = 110 },
                },
                new MotorModel()
                {
                    Name = "mittel",
                    Einlass = new EinlassModel() { SteuerzeitSZ = 140 },
                    Auslass = new AuslassModel() { SteuerzeitSZ = 165 },
                    Ueberstroemer = new UeberstroemerModel() { SteuerzeitSZ = 120 },
                },
                new MotorModel()
                {
                    Name = "lang",
                    Einlass = new EinlassModel() { SteuerzeitSZ = 160 },
                    Auslass = new AuslassModel() { SteuerzeitSZ = 185 },
                    Ueberstroemer = new UeberstroemerModel() { SteuerzeitSZ = 130 },
                },
                new MotorModel()
                {
                    Name = "sehr lang",
                    Einlass = new EinlassModel() { SteuerzeitSZ = 180 },
                    Auslass = new AuslassModel() { SteuerzeitSZ = 205 },
                    Ueberstroemer = new UeberstroemerModel() { SteuerzeitSZ = 140 },
                },
            };

            this.HelperVehicles = new ObservableCollection<VehiclesModel>(_vehicleService.RetrieveVehicles());

            this.InsertReferenceCommand = new RelayCommand(this.InsertReference);
            this.InsertVehicleCommand = new RelayCommand(this.InsertVehicle);
        }

        #region Methods



        /// <summary>
        /// Refreshes the steuerzeit.
        /// </summary>
        /// <returns></returns>
        protected void RefreshSteuerzeit()
        {
            if (this.SteuerzeitEinlass.HasValue)
            {
                this.Einlass_Steuerwinkel_oeffnen = EngineLogic.GetSteuerwinkelOeffnet(this.SteuerzeitEinlass.Value, 0, 0);
                this.Einlass_Steuerwinkel_schließen = EngineLogic.GetSteuerwinkelSchließt(this.SteuerzeitEinlass.Value, 0, 0);
            }

            if (this.SteuerzeitAuslass.HasValue)
            {
                this.Auslass_Steuerwinkel_oeffnen = EngineLogic.GetSteuerwinkelOeffnet(0, this.SteuerzeitAuslass.Value, 0);
                this.Auslass_Steuerwinkel_schließen = EngineLogic.GetSteuerwinkelSchließt(0, this.SteuerzeitAuslass.Value, 0);
            }

            if (this.SteuerzeitUeberstroemer.HasValue)
            {
                this.Ueberstroemer_Steuerwinkel_oeffnen = EngineLogic.GetSteuerwinkelOeffnet(0, 0, this.SteuerzeitUeberstroemer.Value);
                this.Ueberstroemer_Steuerwinkel_schließen = EngineLogic.GetSteuerwinkelSchließt(0, 0, this.SteuerzeitUeberstroemer.Value);
            }

            if (this.SteuerzeitUeberstroemer.HasValue && this.SteuerzeitAuslass.HasValue)
            {
                this.SteuerzeitVorauslass = EngineLogic.GetVorauslass(this.SteuerzeitAuslass.Value, this.SteuerzeitUeberstroemer.Value);
            }

            if (this.SteuerzeitEinlass.HasValue && this.SteuerzeitUeberstroemer.HasValue && this.SteuerzeitAuslass.HasValue)
            {
                Stream stream = SimTuning.Core.Converters.Converts.SKBitmapToStream(bitmap: EngineLogic.GetSteuerdiagramm(this.SteuerzeitEinlass.Value, this.SteuerzeitAuslass.Value, this.SteuerzeitUeberstroemer.Value));
                if (stream != null)
                {
                    this.PortTimingCircle = ImageSource.FromStream(() => stream);
                }
            }
        }

        /// <summary>
        /// Inserts the reference.
        /// </summary>
        private void InsertReference()
        {
            if (this.MotorSteuerzeit.Einlass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitEinlass = this.MotorSteuerzeit.Einlass.SteuerzeitSZ.Value;
            }

            if (this.MotorSteuerzeit.Auslass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitAuslass = this.MotorSteuerzeit.Auslass.SteuerzeitSZ.Value;
            }

            if (this.MotorSteuerzeit.Ueberstroemer.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitUeberstroemer = this.MotorSteuerzeit.Ueberstroemer.SteuerzeitSZ.Value;
            }
        }

        /// <summary>
        /// Inserts the vehicle.
        /// </summary>
        private void InsertVehicle()
        {
            if (this.HelperVehicle.Motor.Einlass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitEinlass = this.HelperVehicle.Motor.Einlass.SteuerzeitSZ;
            }

            if (this.HelperVehicle.Motor.Auslass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitAuslass = this.HelperVehicle.Motor.Auslass.SteuerzeitSZ;
            }

            if (this.HelperVehicle.Motor.Ueberstroemer.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitUeberstroemer = this.HelperVehicle.Motor.Ueberstroemer.SteuerzeitSZ;
            }
        }

        #endregion Methods

        #region Values
        private ImageSource _portTimingCircle;

        public ImageSource PortTimingCircle
        {
            get => _portTimingCircle;
            private set => SetProperty(ref _portTimingCircle, value);
        }

        private readonly ILogger<SteuerdiagrammViewModel> _logger;

        private readonly IVehicleService _vehicleService;

        private double? _auslass_Steuerwinkel_oeffnen;

        private double? _auslass_Steuerwinkel_schließen;

        private double? _einlass_Steuerwinkel_oeffnen;

        private double? _einlass_Steuerwinkel_schließen;

        private VehiclesModel _helperVehicle;

        private ObservableCollection<VehiclesModel> _helperVehicles;

        private Data.Models.MotorModel _motorSteuerzeit;

        private ObservableCollection<Data.Models.MotorModel> _motorSteuerzeiten;

        private double? _steuerzeitAuslass;

        private double? _steuerzeitEinlass;

        private double? _steuerzeitUeberstroemer;

        private double? _steuerzeitVorauslass;

        private double? _ueberstroemer_Steuerwinkel_oeffnen;

        private double? _ueberstroemer_Steuerwinkel_schließen;

        public double? Auslass_Steuerwinkel_oeffnen
        {
            get => this._auslass_Steuerwinkel_oeffnen;
            set => this.SetProperty(ref this._auslass_Steuerwinkel_oeffnen, value);
        }

        public double? Auslass_Steuerwinkel_schließen
        {
            get => this._auslass_Steuerwinkel_schließen;
            set => this.SetProperty(ref this._auslass_Steuerwinkel_schließen, value);
        }

        public double? Einlass_Steuerwinkel_oeffnen
        {
            get => this._einlass_Steuerwinkel_oeffnen;
            set => this.SetProperty(ref this._einlass_Steuerwinkel_oeffnen, value);
        }

        public double? Einlass_Steuerwinkel_schließen
        {
            get => this._einlass_Steuerwinkel_schließen;
            set => this.SetProperty(ref this._einlass_Steuerwinkel_schließen, value);
        }

        public VehiclesModel HelperVehicle
        {
            get => this._helperVehicle;
            set => this.SetProperty(ref this._helperVehicle, value);
        }

        public ObservableCollection<VehiclesModel> HelperVehicles
        {
            get => this._helperVehicles;
            set => this.SetProperty(ref this._helperVehicles, value);
        }

        /// <summary>
        /// Gets or sets the insert reference command.
        /// </summary>
        /// <value>The insert reference command.</value>
        public IRelayCommand InsertReferenceCommand { get; set; }

        /// <summary>
        /// Gets or sets the insert vehicle command.
        /// </summary>
        /// <value>The insert vehicle command.</value>
        public IRelayCommand InsertVehicleCommand { get; set; }

        public Data.Models.MotorModel MotorSteuerzeit
        {
            get => this._motorSteuerzeit;
            set => this.SetProperty(ref this._motorSteuerzeit, value);
        }

        public ObservableCollection<Data.Models.MotorModel> MotorSteuerzeiten
        {
            get => this._motorSteuerzeiten;
            set => this.SetProperty(ref this._motorSteuerzeiten, value);
        }

        /// <summary>
        /// Gets or sets the steuerzeit auslass.
        /// </summary>
        /// <value>The steuerzeit auslass.</value>
        public double? SteuerzeitAuslass
        {
            get => this._steuerzeitAuslass;
            set
            {
                this.SetProperty(ref this._steuerzeitAuslass, value);
                this.RefreshSteuerzeit();
            }
        }

        /// <summary>
        /// Gets or sets the steuerzeit einlass.
        /// </summary>
        /// <value>The steuerzeit einlass.</value>
        public double? SteuerzeitEinlass
        {
            get => this._steuerzeitEinlass;
            set
            {
                this.SetProperty(ref this._steuerzeitEinlass, value);
                this.RefreshSteuerzeit();
            }
        }

        /// <summary>
        /// Gets or sets the steuerzeit ueberstroemer.
        /// </summary>
        /// <value>The steuerzeit ueberstroemer.</value>
        public double? SteuerzeitUeberstroemer
        {
            get => this._steuerzeitUeberstroemer;
            set
            {
                this.SetProperty(ref this._steuerzeitUeberstroemer, value);
                this.RefreshSteuerzeit();
            }
        }

        public double? SteuerzeitVorauslass
        {
            get => this._steuerzeitVorauslass;
            set => this.SetProperty(ref this._steuerzeitVorauslass, value);
        }

        public double? Ueberstroemer_Steuerwinkel_oeffnen
        {
            get => this._ueberstroemer_Steuerwinkel_oeffnen;
            set => this.SetProperty(ref this._ueberstroemer_Steuerwinkel_oeffnen, value);
        }

        public double? Ueberstroemer_Steuerwinkel_schließen
        {
            get => this._ueberstroemer_Steuerwinkel_schließen;
            set => this.SetProperty(ref this._ueberstroemer_Steuerwinkel_schließen, value);
        }

        #endregion Values
    }
}