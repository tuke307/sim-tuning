// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimTuning.Data;
using SimTuning.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimTuning.Core.Services
{
    /// <inheritdoc cref="IVehicleService" />
    public class VehicleService : IVehicleService
    {
        private readonly ILogger<VehicleService> _logger;

        private List<DynoModel> Dynos { get; set; }

        private List<EnvironmentModel> Environments { get; set; }

        private List<MotorModel> Motoren { get; set; }

        private List<VehiclesModel> Vehicles { get; set; }

        public VehicleService(ILogger<VehicleService> logger)
        {
            _logger = logger;

            Dynos = new List<DynoModel>();
            Vehicles = new List<VehiclesModel>();
            Environments = new List<EnvironmentModel>();
            Motoren = new List<MotorModel>();
        }

        /// <inheritdoc />
        public DynoModel CreateOne(DynoModel dyno)
        {
            try
            {
                using var db = new DatabaseContext();
                db.Dyno.Add(dyno);
                db.SaveChanges();

                Dynos.Add(dyno);

                _logger.LogInformation("Dyno: {DynoName} (ID: {DynoId}) created.", dyno.Name, dyno.Id);
                return dyno;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error creating dyno: {DynoName}", dyno.Name);
                return null;
            }
        }

        /// <inheritdoc />
        public VehiclesModel CreateOne(VehiclesModel vehicle)
        {
            try
            {
                using var db = new DatabaseContext();
                db.Vehicles.Add(vehicle);
                db.SaveChanges();

                Vehicles.Add(vehicle);

                _logger.LogInformation("Vehicle: {VehicleName} (ID: {VehicleId}) created.", vehicle.Name, vehicle.Id);
                return vehicle;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error creating vehicle: {VehicleName}", vehicle.Name);
                return null;
            }
        }

        /// <inheritdoc />
        public void Delete(List<AusrollenModel> ausrollen)
        {
            // Remove entities with valid IDs only
            var validItems = ausrollen.Where(item => item.Id.HasValue && item.Id.Value != 0).ToList();

            if (!validItems.Any())
                return;

            using var db = new DatabaseContext();
            db.Ausrollen.RemoveRange(validItems);
            db.SaveChanges();
        }

        /// <inheritdoc />
        public void Delete(List<GeschwindigkeitModel> geschwindigkeit)
        {
            // Remove entities with valid IDs only
            var validItems = geschwindigkeit.Where(item => item.Id.HasValue && item.Id.Value != 0).ToList();

            if (!validItems.Any())
                return;

            using var db = new DatabaseContext();
            db.Geschwindigkeit.RemoveRange(validItems);
            db.SaveChanges();
        }

        /// <inheritdoc />
        public void DeleteOne(VehiclesModel vehicle)
        {
            if (vehicle.Id == null || vehicle.Id == 0)
            {
                return;
            }

            using (var db = new DatabaseContext())
            {
                var existingVehicle = db.Vehicles.Find(vehicle.Id);
                if (existingVehicle != null)
                {
                    db.Vehicles.Remove(existingVehicle);
                    db.SaveChanges();

                    // Remove from local list
                    Vehicles.Remove(Vehicles.Single(d => d.Id == vehicle.Id));

                    _logger.LogInformation("Vehicle: {0} (ID: {1}) deleted.", vehicle.Name, vehicle.Id);
                }
                else
                {
                    _logger.LogWarning("Vehicle with ID: {0} not found in database.", vehicle.Id);
                }
            }
        }

        /// <inheritdoc />
        public void DeleteOne(DynoModel dyno)
        {
            if (dyno.Id == null || dyno.Id == 0)
            {
                return;
            }

            using var db = new DatabaseContext();
            var existingDyno = db.Dyno.Find(dyno.Id);
            if (existingDyno != null)
            {
                db.Dyno.Remove(existingDyno);
                db.SaveChanges();

                Dynos.Remove(Dynos.Single(d => d.Id == dyno.Id));

                _logger.LogInformation("Dyno: {DynoName} (ID: {DynoId}) deleted.", dyno.Name, dyno.Id);
            }
            else
            {
                _logger.LogWarning("Dyno with ID: {DynoId} not found in database.", dyno.Id);
            }
        }

        /// <inheritdoc />
        public List<DynoModel> RetrieveDynos(bool forceupdate = false)
        {
            try
            {
                if (Dynos.Count == 0 || forceupdate)
                {
                    using var db = new DatabaseContext();
                    Dynos = db.Dyno
                        .Include(dyno => dyno.Vehicle)
                        .Include(dyno => dyno.Drehzahl)
                        .Include(dyno => dyno.DynoPS)
                        .ToList();
                }

                _logger.LogInformation("all Dynos retrieved.");

                return Dynos;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }

            return null;
        }

        /// <inheritdoc />
        public List<EnvironmentModel> RetrieveEnvironments(bool forceupdate = false)
        {
            try
            {
                if (Environments.Count == 0 || forceupdate)
                {
                    using var db = new DatabaseContext();
                    Environments = db.Environment.ToList();
                }

                _logger.LogInformation("all Environments retrieved.");
                return Environments;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error retrieving environments");
                return null;
            }
        }

        /// <inheritdoc />
        public List<MotorModel> RetrieveMotoren(bool forceupdate = false)
        {
            try
            {
                if (Motoren.Count == 0 || forceupdate)
                {
                    using var db = new DatabaseContext();
                    Motoren = db.Motor.ToList();
                }

                _logger.LogInformation("all Motoren retrieved.");
                return Motoren;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }

            return null;
        }

        /// <inheritdoc />
        public VehiclesModel RetrieveOne(int? id)
        {
            if (!id.HasValue)
                return null;

            _logger.LogInformation("Vehicle (ID: {1}) retrieved.", id);

            return Vehicles.Single(d => d.Id == id);
        }

        /// <inheritdoc />
        public DynoModel RetrieveOneActive()
        {
            _logger.LogInformation("retrieve active Vehicle.");

            return Dynos.SingleOrDefault(d => d.Active == true);
        }

        /// <inheritdoc />
        public List<VehiclesModel> RetrieveVehicles(bool forceupdate = false)
        {
            try
            {
                if (Vehicles.Count == 0 || forceupdate)
                {
                    using var db = new DatabaseContext();
                    Vehicles = db.Vehicles
                        .Include(vehicle => vehicle.Motor)
                            .ThenInclude(motor => motor.Auslass)
                                .ThenInclude(auslass => auslass.Auspuff)
                        .Include(vehicle => vehicle.Motor)
                            .ThenInclude(motor => motor.Einlass)
                                .ThenInclude(einlass => einlass.Vergaser)
                        .Include(vehicle => vehicle.Motor)
                            .ThenInclude(motor => motor.Ueberstroemer)
                        .Include(vehicle => vehicle.Dyno)
                            .ThenInclude(dyno => dyno.Ausrollen)
                        .Include(vehicle => vehicle.Dyno)
                            .ThenInclude(dyno => dyno.Geschwindigkeit)
                        .Include(vehicle => vehicle.Dyno)
                            .ThenInclude(dyno => dyno.DynoPS)
                        .ToList();
                }

                _logger.LogInformation("retrieve all Vehicles.");
                return Vehicles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error retrieving vehicles");
                return null;
            }
        }

        /// <inheritdoc />
        public void UpdateOne(VehiclesModel vehicle)
        {
            try
            {
                if (vehicle.Id == null || vehicle.Id == 0)
                {
                    return;
                }

                int index = Vehicles.FindIndex(v => v.Id == vehicle.Id);
                if (index >= 0)
                {
                    Vehicles[index] = vehicle;
                }

                using var db = new DatabaseContext();
                db.Vehicles.Attach(vehicle);
                db.SaveChanges();

                _logger.LogInformation("Vehicle {VehicleName} (ID: {VehicleId}) updated.", vehicle.Name, vehicle.Id);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error updating vehicle: {VehicleName}", vehicle.Name);
            }
        }

        /// <inheritdoc />
        public void UpdateOne(DynoModel dyno)
        {
            try
            {
                if (dyno.Id == null || dyno.Id == 0)
                {
                    return;
                }

                int index = Dynos.FindIndex(d => d.Id == dyno.Id);
                if (index >= 0)
                {
                    Dynos[index] = dyno;
                }

                using var db = new DatabaseContext();
                db.Dyno.Attach(dyno);
                db.SaveChanges();

                _logger.LogInformation("Dyno {DynoName} (ID: {DynoId}) updated.", dyno.Name, dyno.Id);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error updating dyno: {DynoName}", dyno.Name);
            }
        }
    }
}