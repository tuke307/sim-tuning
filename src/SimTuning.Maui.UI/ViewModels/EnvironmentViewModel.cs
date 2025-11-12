// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Models;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using System.Collections.ObjectModel;

namespace SimTuning.Maui.UI.ViewModels
{
    public class EnvironmentViewModel : ViewModelBase
    {
        public EnvironmentViewModel(
            ILogger<EnvironmentViewModel> logger,
            IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
        }

        #region Values

        protected readonly IVehicleService _vehicleService;

        private readonly ILogger<EnvironmentViewModel> _logger;

        private Data.Models.MotorModel _engine;
        private ObservableCollection<Data.Models.MotorModel> _engines;

        public MotorModel Engine
        {
            get => _engine;
            set
            {
                // Einfügen
                SetProperty(ref _engine, value);

                OnPropertyChanged(nameof(VehicleMotorAuslassSteuerzeitSZ));
                OnPropertyChanged(nameof(VehicleMotorEinlassSteuerzeitSZ));
                OnPropertyChanged(nameof(VehicleMotorUeberstroemerSteuerzeitSZ));
            }
        }

        public ObservableCollection<MotorModel> Engines
        {
            get => _engines;
            set => SetProperty(ref _engines, value);
        }

        public double? VehicleMotorAuslassSteuerzeitSZ
        {
            get => Engine?.Auslass?.SteuerzeitSZ;
            set
            {
                if (Engine?.Auslass == null)
                {
                    return;
                }
                Engine.Auslass.SteuerzeitSZ = value;
            }
        }

        public double? VehicleMotorEinlassSteuerzeitSZ
        {
            get => Engine?.Einlass?.SteuerzeitSZ;
            set
            {
                if (Engine?.Einlass == null)
                {
                    return;
                }
                Engine.Einlass.SteuerzeitSZ = value;
            }
        }

        public double? VehicleMotorUeberstroemerSteuerzeitSZ
        {
            get => Engine?.Ueberstroemer?.SteuerzeitSZ;
            set
            {
                if (Engine?.Ueberstroemer == null)
                {
                    return;
                }
                Engine.Ueberstroemer.SteuerzeitSZ = value;
            }
        }

        #endregion Values
    }
}