// Copyright (c) 2021 tuke productions. All rights reserved.
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
            this._logger = logger;
            this._vehicleService = vehicleService;
        }

        #region Values

        protected readonly IVehicleService _vehicleService;

        private readonly ILogger<EnvironmentViewModel> _logger;

        private Data.Models.MotorModel _engine;
        private ObservableCollection<Data.Models.MotorModel> _engines;

        public MotorModel Engine
        {
            get => this._engine;
            set
            {
                // Einfügen
                this.SetProperty(ref this._engine, value);

                OnPropertyChanged(nameof(VehicleMotorAuslassSteuerzeitSZ));
                OnPropertyChanged(nameof(VehicleMotorEinlassSteuerzeitSZ));
                OnPropertyChanged(nameof(VehicleMotorUeberstroemerSteuerzeitSZ));
            }
        }

        public ObservableCollection<MotorModel> Engines
        {
            get => this._engines;
            set => this.SetProperty(ref this._engines, value);
        }

        public double? VehicleMotorAuslassSteuerzeitSZ
        {
            get => this.Engine?.Auslass?.SteuerzeitSZ;
            set
            {
                if (this.Engine?.Auslass == null)
                {
                    return;
                }
                this.Engine.Auslass.SteuerzeitSZ = value;
            }
        }

        public double? VehicleMotorEinlassSteuerzeitSZ
        {
            get => this.Engine?.Einlass?.SteuerzeitSZ;
            set
            {
                if (this.Engine?.Einlass == null)
                {
                    return;
                }
                this.Engine.Einlass.SteuerzeitSZ = value;
            }
        }

        public double? VehicleMotorUeberstroemerSteuerzeitSZ
        {
            get => this.Engine?.Ueberstroemer?.SteuerzeitSZ;
            set
            {
                if (this.Engine?.Ueberstroemer == null)
                {
                    return;
                }
                this.Engine.Ueberstroemer.SteuerzeitSZ = value;
            }
        }

        #endregion Values
    }
}