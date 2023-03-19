// Copyright (c) 2021 tuke productions. All rights reserved.
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
            this._logger = logger;

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
                using (var db = new DatabaseContext())
                {
                    // in Datenbank einfügen
                    db.Dyno.Add(dyno);
                    db.SaveChanges();
                }

                // in lokaler liste hinzufügen
                this.Dynos.Add(dyno);

                _logger.LogInformation("Dyno: {0} (ID: {1}) created.", dyno.Name, dyno.Id);
                return dyno;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }

            return null;
        }

        /// <inheritdoc />
        public VehiclesModel CreateOne(VehiclesModel vehicle)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // in Datenbank einfügen
                    db.Vehicles.Add(vehicle);
                    db.SaveChanges();
                }

                // in lokaler liste hinzufügen
                this.Vehicles.Add(vehicle);

                _logger.LogInformation("Vehicle: {0} (ID: {1}) created.", vehicle.Name, vehicle.Id);

                return vehicle;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }

            return null;
        }

        /// <inheritdoc />
        public void Delete(List<AusrollenModel> ausrollen)
        {
            // Entitys mit keinen ID`s löschen.
            foreach (var item in ausrollen)
            {
                if (item.Id == null || item.Id == 0)
                    ausrollen.Remove(item);
            }

            using (var db = new DatabaseContext())
            {
                db.Ausrollen.RemoveRange(ausrollen);
                db.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void Delete(List<GeschwindigkeitModel> geschwindigkeit)
        {
            // Entitys mit keinen ID`s löschen.
            foreach (var item in geschwindigkeit)
            {
                if (item.Id == null || item.Id == 0)
                    geschwindigkeit.Remove(item);
            }

            using (var db = new DatabaseContext())
            {
                db.Geschwindigkeit.RemoveRange(geschwindigkeit);
                db.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void DeleteOne(VehiclesModel vehicle)
        {
            // wenn keine ID.
            if (vehicle.Id == null || vehicle.Id == 0)
            {
                return;
            }

            using (var db = new DatabaseContext())
            {
                // TODO: Database operation expected to affect 1 row(s) but actually affected 0 row(s).
                db.Vehicles.Remove(vehicle);
                db.SaveChanges();
            }

            // in lokaler liste löschen
            this.Vehicles.Remove(this.Vehicles.Single(d => d.Id == vehicle.Id));

            _logger.LogInformation("Vehicle: {0} (ID: {1}) deleted.", vehicle.Name, vehicle.Id);
        }

        /// <inheritdoc />
        public void DeleteOne(DynoModel dyno)
        {
            try
            {
                // wenn keine ID.
                if (dyno.Id == null || dyno.Id == 0)
                {
                    return;
                }

                using (var db = new Data.DatabaseContext())
                {
                    // in Datenbank löschen
                    db.Dyno.Remove(dyno);

                    db.SaveChanges();
                }

                // in lokaler liste löschen
                this.Dynos.Remove(this.Dynos.Single(d => d.Id == dyno.Id));

                _logger.LogInformation("Dyno: {0} (ID: {1}) deleted.", dyno.Name, dyno.Id);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
        }

        /// <inheritdoc />
        public List<DynoModel> RetrieveDynos(bool forceupdate = false)
        {
            try
            {
                if (Dynos.Count == 0 || forceupdate)
                {
                    using (var db = new DatabaseContext())
                    {
                        this.Dynos = db.Dyno
                            // Vehicle
                            .Include(dyno => dyno.Vehicle)
                            .Include(dyno => dyno.Drehzahl)
                            .Include(dyno => dyno.DynoPS)
                            //.Include(dyno => dyno.DynoNm)
                            .ToList();
                    }
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
                    using (var db = new DatabaseContext())
                    {
                        this.Environments = db.Environment
                            .ToList();
                    }
                }

                _logger.LogInformation("all Environments retrieved.");

                return Environments;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }

            return null;
        }

        /// <inheritdoc />
        public List<MotorModel> RetrieveMotoren(bool forceupdate = false)
        {
            try
            {
                if (Motoren.Count == 0 || forceupdate)
                {
                    using (var db = new DatabaseContext())
                    {
                        this.Motoren = db.Motor.ToList();
                    }
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
                    using (var db = new DatabaseContext())
                    {
                        this.Vehicles = db.Vehicles
                            // Motor
                            .Include(vehicle => vehicle.Motor)
                            // Dyno
                            .Include(vehicle => vehicle.Dyno)
                            .Include(vehicle => vehicle.Dyno)
                                .ThenInclude(dyno => dyno.Ausrollen)
                            .Include(vehicle => vehicle.Dyno)
                                .ThenInclude(dyno => dyno.Geschwindigkeit)
                             .Include(vehicle => vehicle.Dyno)
                                .ThenInclude(dyno => dyno.DynoPS)
                            // Motor
                            .Include(vehicle => vehicle.Motor)
                                .ThenInclude(motor => motor.Auslass)
                                    .ThenInclude(auslass => auslass.Auspuff)
                             .Include(vehicle => vehicle.Motor)
                                .ThenInclude(motor => motor.Einlass)
                                    .ThenInclude(einlass => einlass.Vergaser)
                             .Include(vehicle => vehicle.Motor)
                                .ThenInclude(motor => motor.Ueberstroemer)
                            .ToList();
                    }
                }

                _logger.LogInformation("retrieve all Vehicles.");

                return Vehicles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }

            return null;
        }

        /// <inheritdoc />
        public void UpdateOne(VehiclesModel vehicle)
        {
            try
            {
                // wenn keine ID => Vehicle muss erst erstellt werden.
                if (vehicle.Id == null || vehicle.Id == 0)
                {
                    return;
                }

                // in lokaler liste ersetzen
                this.Vehicles[this.Vehicles.FindIndex(v => v.Id == vehicle.Id)] = vehicle;

                using (var db = new DatabaseContext())
                {
                    // in Datenbank einfügen
                    db.Vehicles.Attach(vehicle);
                    db.SaveChanges();
                }

                _logger.LogInformation("Vehicle {0} (ID: {1}) updated.", vehicle.Name, vehicle.Id);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
        }

        /// <inheritdoc />
        public void UpdateOne(DynoModel dyno)
        {
            try
            {
                // wenn keine ID => Dyno muss erst erstellt werden.
                if (dyno.Id == null || dyno.Id == 0)
                {
                    return;
                }

                // in lokaler liste ersetzen
                this.Dynos[this.Dynos.FindIndex(d => d.Id == dyno.Id)] = dyno;

                using (var db = new DatabaseContext())
                {
                    // in Datenbank einfügen
                    db.Dyno.Attach(dyno);
                    db.SaveChanges();
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
        }
    }
}