// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SimTuning.Data;
using SimTuning.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace SimTuning.Core.Services
{
    /// <inheritdoc cref="IVehicleService" />
    public class VehicleService : IVehicleService
    {
        private readonly DatabaseContext _context;

        private List<DynoModel> Dynos { get; set; }

        private List<EnvironmentModel> Environments { get; set; }

        private List<MotorModel> Motoren { get; set; }

        private List<VehiclesModel> Vehicles { get; set; }

        public VehicleService()
        {
            this._context = Ioc.Default.GetRequiredService<DatabaseContext>();

            Dynos = new List<DynoModel>();
            Vehicles = new List<VehiclesModel>();
            Environments = new List<EnvironmentModel>();
            Motoren = new List<MotorModel>();
        }

        /// <inheritdoc />
        public DynoModel CreateOne(DynoModel dyno)
        {
            using (var db = new DatabaseContext())
            {
                // in Datenbank einfügen
                db.Dyno.Add(dyno);
                db.SaveChanges();
            }

            // in lokaler liste hinzufügen
            this.Dynos.Add(dyno);

            return dyno;
        }

        /// <inheritdoc />
        public VehiclesModel CreateOne(VehiclesModel vehicle)
        {
            using (var db = new DatabaseContext())
            {
                // in Datenbank einfügen
                db.Vehicles.Add(vehicle);
                db.SaveChanges();
            }

            // in lokaler liste hinzufügen
            this.Vehicles.Add(vehicle);

            return vehicle;
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
        }

        /// <inheritdoc />
        public void DeleteOne(DynoModel dyno)
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
        }

        /// <inheritdoc />
        public List<DynoModel> RetrieveDynos(bool forceupdate = false)
        {
            if (Dynos.Count == 0 || forceupdate)
            {
                using (var db = new DatabaseContext())
                {
                    this.Dynos = db.Dyno
                        .ToList();
                }
            }

            return Dynos;
        }

        /// <inheritdoc />
        public List<EnvironmentModel> RetrieveEnvironments(bool forceupdate = false)
        {
            if (Environments.Count == 0 || forceupdate)
            {
                using (var db = new DatabaseContext())
                {
                    this.Environments = db.Environment
                        .ToList();
                }
            }

            return Environments;
        }

        /// <inheritdoc />
        public List<MotorModel> RetrieveMotoren(bool forceupdate = false)
        {
            if (Motoren.Count == 0 || forceupdate)
            {
                using (var db = new DatabaseContext())
                {
                    this.Motoren = db.Motor.ToList();
                }
            }

            return Motoren;
        }

        /// <inheritdoc />
        public VehiclesModel RetrieveOne(int? id)
        {
            if (!id.HasValue)
                return null;

            return Vehicles.Single(d => d.Id == id);
        }

        /// <inheritdoc />
        public DynoModel RetrieveOneActive()
        {
            return Dynos.SingleOrDefault(d => d.Active == true);
        }

        /// <inheritdoc />
        public List<VehiclesModel> RetrieveVehicles(bool forceupdate = false)
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
                    // Tuning
                        .Include(vehicle => vehicle.Tuning)
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

            return Vehicles;
        }

        /// <inheritdoc />
        public void UpdateOne(VehiclesModel vehicle)
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
        }

        /// <inheritdoc />
        public void UpdateOne(DynoModel dyno)
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
    }
}