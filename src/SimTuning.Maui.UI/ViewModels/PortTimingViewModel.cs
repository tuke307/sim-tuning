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
    public class PortTimingViewModel : ViewModelBase
    {
        public PortTimingViewModel(
            ILogger<PortTimingViewModel> logger,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._vehicleService = vehicleService;

            // lokale liste kreieren
            Engines = new ObservableCollection<MotorModel>()
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
        }

        #region Values

        protected readonly IVehicleService _vehicleService;

        private readonly ILogger<PortTimingViewModel> _logger;

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