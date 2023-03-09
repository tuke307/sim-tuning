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
    public class EinstellungenVehiclesViewModel : VehiclesViewModel
    {
        public EinstellungenVehiclesViewModel(
            ILogger<EinstellungenVehiclesViewModel> logger,
            IVehicleService vehicleService)
            : base(logger, vehicleService)
        {
            this._logger = logger;

            this.NewVehicleCommand = new RelayCommand(this.NewVehicle);
            this.DeleteVehicleCommand = new RelayCommand(this.DeleteVehicle);
            this.SaveVehicleCommand = new RelayCommand(this.SaveVehicle);
        }

        #region Methods

        /// <summary>
        /// Deletes the vehicle.
        /// </summary>
        protected void DeleteVehicle()
        {
            try
            {
                if (this.Vehicle.Deletable)
                {
                    // in Datenbank löschen
                    _vehicleService.DeleteOne(this.Vehicle);

                    // in lokaler liste löschen
                    this.Vehicles.Remove(this.Vehicle);

                    this.Vehicle = null;
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

                this.Vehicles.Add(vehicle);
                this.Vehicle = this.Vehicles.Last();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Saves the vehicle.
        /// </summary>
        protected void SaveVehicle()
        {
            _vehicleService.UpdateOne(this.Vehicle);
        }

        #endregion Methods

        #region Values

        private readonly ILogger<EinstellungenVehiclesViewModel> _logger;

        public IRelayCommand DeleteVehicleCommand { get; set; }

        public IRelayCommand NewVehicleCommand { get; set; }

        public IRelayCommand SaveVehicleCommand { get; set; }

        #endregion Values
    }
}