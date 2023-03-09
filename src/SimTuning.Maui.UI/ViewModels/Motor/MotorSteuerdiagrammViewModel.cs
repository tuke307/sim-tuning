// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Maui.Controls;
    using SimTuning.Core.ModuleLogic;
    using SimTuning.Core.Services;
    using SimTuning.Data;
    using SimTuning.Data.Models;
    using SimTuning.Maui.UI.Services;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class MotorSteuerdiagrammViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MotorSteuerdiagrammViewModel" /> class.
        /// </summary>
        /// <param name="logger"><inheritdoc cref="ILogger" path="/summary/node()" /></param>
        /// <param name="INavigationService"><inheritdoc cref="INavigationService" path="/summary/node()" /></param>
        /// <param name="vehicleService"><inheritdoc cref="IVehicleService" path="/summary/node()" /></param>
        public MotorSteuerdiagrammViewModel(
            ILogger<MotorSteuerdiagrammViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._vehicleService = vehicleService;
        }

        #region Methods

        /// <summary>
        /// Inserts the reference.
        /// </summary>
        public void InsertHelperEngines(MotorModel helperEngine)
        {
            if (helperEngine.Einlass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitEinlass = helperEngine.Einlass.SteuerzeitSZ.Value;
            }

            if (helperEngine.Auslass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitAuslass = helperEngine.Auslass.SteuerzeitSZ.Value;
            }

            if (helperEngine.Ueberstroemer.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitUeberstroemer = helperEngine.Ueberstroemer.SteuerzeitSZ.Value;
            }
        }

        /// <summary>
        /// Inserts the vehicle.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.Einlass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitEinlass = helperVehicle.Motor.Einlass.SteuerzeitSZ;
            }

            if (helperVehicle.Motor.Auslass.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitAuslass = helperVehicle.Motor.Auslass.SteuerzeitSZ;
            }

            if (helperVehicle.Motor.Ueberstroemer.SteuerzeitSZ.HasValue)
            {
                this.SteuerzeitUeberstroemer = helperVehicle.Motor.Ueberstroemer.SteuerzeitSZ;
            }
        }

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

        #endregion Methods

        #region Values

        private readonly ILogger<MotorSteuerdiagrammViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private double? _auslass_Steuerwinkel_oeffnen;
        private double? _auslass_Steuerwinkel_schließen;
        private double? _einlass_Steuerwinkel_oeffnen;
        private double? _einlass_Steuerwinkel_schließen;

        private ImageSource _portTimingCircle;

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

        public ImageSource PortTimingCircle
        {
            get => _portTimingCircle;
            private set => SetProperty(ref _portTimingCircle, value);
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