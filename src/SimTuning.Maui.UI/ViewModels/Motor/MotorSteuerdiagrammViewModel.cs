// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.ViewModels
{
    using CommunityToolkit.Maui;
    using CommunityToolkit.Maui.Views;
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

    public partial class MotorSteuerdiagrammViewModel : ViewModelBase
    {
        private readonly IPopupService _popupService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MotorSteuerdiagrammViewModel" /> class.
        /// </summary>
        /// <param name="logger"><inheritdoc cref="ILogger" path="/summary/node()" /></param>
        /// <param name="INavigationService"><inheritdoc cref="INavigationService" path="/summary/node()" /></param>
        /// <param name="vehicleService"><inheritdoc cref="IVehicleService" path="/summary/node()" /></param>
        public MotorSteuerdiagrammViewModel(
            ILogger<MotorSteuerdiagrammViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService,
            IPopupService popupService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _popupService = popupService;
        }

        #region Methods

        /// <summary>
        /// Inserts the reference.
        /// </summary>
        public void InsertHelperEngines(MotorModel helperEngine)
        {
            if (helperEngine.Einlass.SteuerzeitSZ.HasValue)
            {
                SteuerzeitEinlass = helperEngine.Einlass.SteuerzeitSZ.Value;
            }

            if (helperEngine.Auslass.SteuerzeitSZ.HasValue)
            {
                SteuerzeitAuslass = helperEngine.Auslass.SteuerzeitSZ.Value;
            }

            if (helperEngine.Ueberstroemer.SteuerzeitSZ.HasValue)
            {
                SteuerzeitUeberstroemer = helperEngine.Ueberstroemer.SteuerzeitSZ.Value;
            }
        }

        /// <summary>
        /// Inserts the vehicle.
        /// </summary>
        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.Einlass.SteuerzeitSZ.HasValue)
            {
                SteuerzeitEinlass = helperVehicle.Motor.Einlass.SteuerzeitSZ;
            }

            if (helperVehicle.Motor.Auslass.SteuerzeitSZ.HasValue)
            {
                SteuerzeitAuslass = helperVehicle.Motor.Auslass.SteuerzeitSZ;
            }

            if (helperVehicle.Motor.Ueberstroemer.SteuerzeitSZ.HasValue)
            {
                SteuerzeitUeberstroemer = helperVehicle.Motor.Ueberstroemer.SteuerzeitSZ;
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

        [RelayCommand]
        private async Task ShowHelperEnginesAsync()
        {
            var result = await _popupService.ShowPopupAsync<PortTimingViewModel>(Shell.Current);
            if (result is MotorModel motorModel)
            {
                InsertHelperEngines(motorModel);
            }
        }

        /// <summary>
        /// Refreshes the steuerzeit.
        /// </summary>
        /// <returns></returns>
        protected void RefreshSteuerzeit()
        {
            if (SteuerzeitEinlass.HasValue)
            {
                Einlass_Steuerwinkel_oeffnen = EngineLogic.GetSteuerwinkelOeffnet(SteuerzeitEinlass.Value, 0, 0);
                Einlass_Steuerwinkel_schließen = EngineLogic.GetSteuerwinkelSchließt(SteuerzeitEinlass.Value, 0, 0);
            }

            if (SteuerzeitAuslass.HasValue)
            {
                Auslass_Steuerwinkel_oeffnen = EngineLogic.GetSteuerwinkelOeffnet(0, SteuerzeitAuslass.Value, 0);
                Auslass_Steuerwinkel_schließen = EngineLogic.GetSteuerwinkelSchließt(0, SteuerzeitAuslass.Value, 0);
            }

            if (SteuerzeitUeberstroemer.HasValue)
            {
                Ueberstroemer_Steuerwinkel_oeffnen = EngineLogic.GetSteuerwinkelOeffnet(0, 0, SteuerzeitUeberstroemer.Value);
                Ueberstroemer_Steuerwinkel_schließen = EngineLogic.GetSteuerwinkelSchließt(0, 0, SteuerzeitUeberstroemer.Value);
            }

            if (SteuerzeitUeberstroemer.HasValue && SteuerzeitAuslass.HasValue)
            {
                SteuerzeitVorauslass = EngineLogic.GetVorauslass(SteuerzeitAuslass.Value, SteuerzeitUeberstroemer.Value);
            }

            if (SteuerzeitEinlass.HasValue && SteuerzeitUeberstroemer.HasValue && SteuerzeitAuslass.HasValue)
            {
                Stream stream = SimTuning.Core.Converters.Converts.SKBitmapToStream(bitmap: EngineLogic.GetSteuerdiagramm(SteuerzeitEinlass.Value, SteuerzeitAuslass.Value, SteuerzeitUeberstroemer.Value));
                if (stream != null)
                {
                    PortTimingCircle = ImageSource.FromStream(() => stream);
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
            get => _auslass_Steuerwinkel_oeffnen;
            set => SetProperty(ref _auslass_Steuerwinkel_oeffnen, value);
        }

        public double? Auslass_Steuerwinkel_schließen
        {
            get => _auslass_Steuerwinkel_schließen;
            set => SetProperty(ref _auslass_Steuerwinkel_schließen, value);
        }

        public double? Einlass_Steuerwinkel_oeffnen
        {
            get => _einlass_Steuerwinkel_oeffnen;
            set => SetProperty(ref _einlass_Steuerwinkel_oeffnen, value);
        }

        public double? Einlass_Steuerwinkel_schließen
        {
            get => _einlass_Steuerwinkel_schließen;
            set => SetProperty(ref _einlass_Steuerwinkel_schließen, value);
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
            get => _steuerzeitAuslass;
            set
            {
                SetProperty(ref _steuerzeitAuslass, value);
                RefreshSteuerzeit();
            }
        }

        /// <summary>
        /// Gets or sets the steuerzeit einlass.
        /// </summary>
        /// <value>The steuerzeit einlass.</value>
        public double? SteuerzeitEinlass
        {
            get => _steuerzeitEinlass;
            set
            {
                SetProperty(ref _steuerzeitEinlass, value);
                RefreshSteuerzeit();
            }
        }

        /// <summary>
        /// Gets or sets the steuerzeit ueberstroemer.
        /// </summary>
        /// <value>The steuerzeit ueberstroemer.</value>
        public double? SteuerzeitUeberstroemer
        {
            get => _steuerzeitUeberstroemer;
            set
            {
                SetProperty(ref _steuerzeitUeberstroemer, value);
                RefreshSteuerzeit();
            }
        }

        public double? SteuerzeitVorauslass
        {
            get => _steuerzeitVorauslass;
            set => SetProperty(ref _steuerzeitVorauslass, value);
        }

        public double? Ueberstroemer_Steuerwinkel_oeffnen
        {
            get => _ueberstroemer_Steuerwinkel_oeffnen;
            set => SetProperty(ref _ueberstroemer_Steuerwinkel_oeffnen, value);
        }

        public double? Ueberstroemer_Steuerwinkel_schließen
        {
            get => _ueberstroemer_Steuerwinkel_schließen;
            set => SetProperty(ref _ueberstroemer_Steuerwinkel_schließen, value);
        }

        #endregion Values
    }
}
