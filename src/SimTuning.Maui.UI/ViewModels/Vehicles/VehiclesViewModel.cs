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
    public class VehiclesViewModel : VehiclesViewModelBase
    {
        public VehiclesViewModel(
            ILogger<VehiclesViewModel> logger,
            IVehicleService vehicleService)
            : base(logger, vehicleService)
        {
            _logger = logger;

            NewVehicleCommand = new RelayCommand(NewVehicle);
            DeleteVehicleCommand = new RelayCommand(DeleteVehicle);
            SaveVehicleCommand = new RelayCommand(SaveVehicle);
        }

        #region Methods

        /// <summary>
        /// Deletes the vehicle.
        /// </summary>
        protected void DeleteVehicle()
        {
            try
            {
                if (Vehicle.Deletable)
                {
                    // in Datenbank löschen
                    _vehicleService.DeleteOne(Vehicle);

                    // in lokaler liste löschen
                    Vehicles.Remove(Vehicle);

                    Vehicle = null;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
        }

        /// <summary>
        /// Creates new vehicle.
        /// </summary>
        protected void NewVehicle()
        {
            try
            {
                var vehicle = new VehiclesModel()
                {
                    Name = "Neues Fahrzeug",
                    Beschreibung = "Erstellt am " + DateTime.Now + " über Fahrzeug-Modul",
                    Deletable = true,
                };
                vehicle = _vehicleService.CreateOne(vehicle);

                Vehicles.Add(vehicle);
                Vehicle = Vehicles.Last();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
        }

        /// <summary>
        /// Saves the vehicle.
        /// </summary>
        protected void SaveVehicle()
        {
            _vehicleService.UpdateOne(Vehicle);
        }

        #endregion Methods

        #region Values

        private readonly ILogger<VehiclesViewModel> _logger;

        public IRelayCommand DeleteVehicleCommand { get; set; }

        public IRelayCommand NewVehicleCommand { get; set; }

        public IRelayCommand SaveVehicleCommand { get; set; }

        #endregion Values
    }
}
