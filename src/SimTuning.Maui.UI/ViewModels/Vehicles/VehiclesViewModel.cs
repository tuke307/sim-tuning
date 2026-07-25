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
                var vehicle = Vehicle;
                if (vehicle is null || !vehicle.Deletable)
                {
                    return;
                }

                // in Datenbank löschen
                _vehicleService.DeleteOne(vehicle);

                // in lokaler liste löschen
                Vehicles.Remove(vehicle);

                Vehicle = null;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error deleting vehicle: {Message}", exc.Message);
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
                var saved = _vehicleService.CreateOne(vehicle);
                if (saved is null)
                {
                    return;
                }

                Vehicles.Add(saved);
                Vehicle = Vehicles.Last();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error creating new vehicle: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Saves the vehicle.
        /// </summary>
        protected void SaveVehicle()
        {
            var vehicle = Vehicle;
            if (vehicle is null)
            {
                return;
            }

            _vehicleService.UpdateOne(vehicle);
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
