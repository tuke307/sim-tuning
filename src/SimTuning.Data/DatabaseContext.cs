// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Data
{
    using Microsoft.EntityFrameworkCore;
    using SimTuning.Data.Models;
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Main Funktionalität der DB.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class DatabaseContext : DbContext
    {
        #region DataSets

        /// <summary>
        /// Gets or sets the auspuff.
        /// </summary>
        /// <value>The auspuff.</value>
        public DbSet<AuspuffModel> Auspuff { get; set; }

        /// <summary>
        /// Gets or sets the ausrollen.
        /// </summary>
        /// <value>The ausrollen.</value>
        public DbSet<AusrollenModel> Ausrollen { get; set; }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public DbSet<DynoModel> Dyno { get; set; }

        /// <summary>
        /// Gets or sets the dyno audio.
        /// </summary>
        /// <value>The dyno audio.</value>
        public DbSet<DrehzahlModel> DynoAudio { get; set; }

        /// <summary>
        /// Gets or sets the dyno nm.
        /// </summary>
        /// <value>The dyno nm.</value>
        [Obsolete("obsolete", true)]
        public DbSet<DynoNmModel> DynoNm { get; set; }

        /// <summary>
        /// Gets or sets the dyno ps.
        /// </summary>
        /// <value>The dyno ps.</value>
        public DbSet<DynoPsModel> DynoPs { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>The environment.</value>
        public DbSet<EnvironmentModel> Environment { get; set; }

        /// <summary>
        /// Gets or sets the beschleunigung.
        /// </summary>
        /// <value>The beschleunigung.</value>
        public DbSet<GeschwindigkeitModel> Geschwindigkeit { get; set; }

        /// <summary>
        /// Gets or sets the motor.
        /// </summary>
        /// <value>The motor.</value>
        public DbSet<MotorModel> Motor { get; set; }

        /// <summary>
        /// Gets or sets the motor auslass.
        /// </summary>
        /// <value>The motor auslass.</value>
        public DbSet<AuslassModel> MotorAuslass { get; set; }

        /// <summary>
        /// Gets or sets the motor einlass.
        /// </summary>
        /// <value>The motor einlass.</value>
        public DbSet<EinlassModel> MotorEinlass { get; set; }

        /// <summary>
        /// Gets or sets the motor ueberstroemer.
        /// </summary>
        /// <value>The motor ueberstroemer.</value>
        public DbSet<UeberstroemerModel> MotorUeberstroemer { get; set; }

        /// <summary>
        /// Gets or sets the vehicles.
        /// </summary>
        /// <value>The vehicles.</value>
        public DbSet<VehiclesModel> Vehicles { get; set; }

        /// <summary>
        /// Gets or sets the vergaser.
        /// </summary>
        /// <value>The vergaser.</value>
        public DbSet<VergaserModel> Vergaser { get; set; }

        #endregion DataSets

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext" /> class.
        /// </summary>
        public DatabaseContext()
        {
            // since android 10, database has to be created at the first time
            if (!File.Exists(Data.DatabaseSettings.DatabasePath))
            {
                var fs = File.Create(Data.DatabaseSettings.DatabasePath);
                fs.Dispose();
            }

            Database.Migrate();
            Database.EnsureCreated();
        }

        /// <summary>
        /// Aufgerufen beim speichern der Entitys.
        /// </summary>
        /// <returns>The Number of state entries.</returns>
        public override int SaveChanges()
        {
            var entries = this.ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntityModel && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                // Modified Date
                ((BaseEntityModel)entityEntry.Entity).UpdatedDate = DateTime.Now;

                // Creation Date
                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntityModel)entityEntry.Entity).CreatedDate = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }

        /// <summary>
        /// einmaliger Aufruf.
        /// </summary>
        /// <param name="options">Optionen.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Filename={DatabaseSettings.DatabasePath}");
        }

        /// <summary>
        /// beim kreieren der Models.
        /// </summary>
        /// <param name="modelBuilder">ModelBuilder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Data-Seeding

            modelBuilder.Entity<EnvironmentModel>().HasData(
                new EnvironmentModel { Id = 1, Name = "Frühling", TemperaturT = 10, LuftdruckP = 1010 },
                new EnvironmentModel { Id = 2, Name = "Sommer", TemperaturT = 25, LuftdruckP = 1010 },
                new EnvironmentModel { Id = 3, Name = "Herbst", TemperaturT = 10, LuftdruckP = 1010 },
                new EnvironmentModel { Id = 4, Name = "Winter", TemperaturT = 1, LuftdruckP = 1010 });

            modelBuilder.Entity<MotorModel>().HasData(
                new MotorModel { Id = 1, Name = "Rh 50 II", HubL = 42.0, PleulL = null, DeachsierungL = null, BohrungD = 38.0, ResonanzU = 5000.0, HubraumV = 47600.0, BrennraumV = null, KurbelgehaeuseV = 142800.0 },
                new MotorModel { Id = 2, Name = "KRo Rh 50", HubL = 42.0, PleulL = null, DeachsierungL = null, BohrungD = 38.0, ResonanzU = 5500.0, HubraumV = 47600.0, BrennraumV = null, KurbelgehaeuseV = 142800.0 },
                new MotorModel { Id = 3, Name = "M53/1 KFR", HubL = 39.5, PleulL = null, DeachsierungL = null, BohrungD = 40.0, ResonanzU = 5750.0, HubraumV = 49600.0, BrennraumV = null, KurbelgehaeuseV = 148800.0 },
                new MotorModel { Id = 4, Name = "M 53/21 KF", HubL = 39.5, PleulL = null, DeachsierungL = null, BohrungD = 40.0, ResonanzU = 5500.0, HubraumV = 49600.0, BrennraumV = null, KurbelgehaeuseV = 148800.0 },
                new MotorModel { Id = 5, Name = "M 541 KF", HubL = 44.0, PleulL = 95.0, DeachsierungL = 2.0, BohrungD = 38.0, ResonanzU = 5500.0, HubraumV = 49900.0, BrennraumV = 5880.0, KurbelgehaeuseV = 149700.0 },
                new MotorModel { Id = 6, Name = "M 741/1 KF", HubL = 44.0, PleulL = null, DeachsierungL = null, BohrungD = 45.0, ResonanzU = 6000.0, HubraumV = 49900.0, BrennraumV = null, KurbelgehaeuseV = 209700.0 });

            modelBuilder.Entity<VehiclesModel>().HasData(
                 new VehiclesModel { Id = 1, MotorId = 1, Name = "SR 2 E", Beschreibung = "Baujahre: 1960 - 1964\nStückzahl: 515.000", Deletable = false, Gewicht = 53.0 },
                 new VehiclesModel { Id = 2, MotorId = 2, Name = "KR 50", Beschreibung = "Baujahre: 1959 - 1964\nStückzahl: 164.500", Deletable = false, Gewicht = 73.0 },
                 new VehiclesModel { Id = 3, MotorId = 3, Name = "KR 51/1 (F)", Beschreibung = "Baujahre: 1968 - 1980\nStückzahl: 375.000", Deletable = false, Gewicht = 80.0 },
                 new VehiclesModel { Id = 4, MotorId = 4, Name = "S 50 B1", Beschreibung = "Baujahre: 1976 - 1980\nStückzahl: 287.000", Deletable = false, Gewicht = 81.0 },
                 new VehiclesModel { Id = 5, MotorId = 5, Name = "S51 B1-4", Beschreibung = "Baujahre: 1980 - 1989\nStückzahl: 360.600", Deletable = false, Gewicht = 79.5/*, Ansaugleitungslaenge = 20*/ },
                 new VehiclesModel { Id = 6, MotorId = 6, Name = "S 70 C", Beschreibung = "Baujahre: 1984 - 1988\nStückzahl: 20.000", Deletable = false, Gewicht = 84.0 });

            modelBuilder.Entity<EinlassModel>().HasData(
                new EinlassModel { Id = 1, MotorId = 1 },
                new EinlassModel { Id = 2, MotorId = 2 },
                new EinlassModel { Id = 3, MotorId = 3, SteuerzeitSZ = 136.0 },
                new EinlassModel { Id = 4, MotorId = 4 },
                new EinlassModel { Id = 5, MotorId = 5, SteuerzeitSZ = 135.0 },
                new EinlassModel { Id = 6, MotorId = 6 });

            modelBuilder.Entity<VergaserModel>().HasData(
                new VergaserModel { Id = 1, EinlassId = 1 },
                new VergaserModel { Id = 2, EinlassId = 2 },
                new VergaserModel { Id = 3, EinlassId = 3 },
                new VergaserModel { Id = 4, EinlassId = 4 },
                new VergaserModel { Id = 5, EinlassId = 5 },
                new VergaserModel { Id = 6, EinlassId = 6 });

            modelBuilder.Entity<AuslassModel>().HasData(
                new AuslassModel { Id = 1, MotorId = 1 },
                new AuslassModel { Id = 2, MotorId = 2 },
                new AuslassModel { Id = 3, MotorId = 3, SteuerzeitSZ = 148.0 },
                new AuslassModel { Id = 4, MotorId = 4 },
                new AuslassModel { Id = 5, MotorId = 5, SteuerzeitSZ = 145.0 },
                new AuslassModel { Id = 6, MotorId = 6 });

            modelBuilder.Entity<UeberstroemerModel>().HasData(
                new UeberstroemerModel { Id = 1, MotorId = 1 },
                new UeberstroemerModel { Id = 2, MotorId = 2 },
                new UeberstroemerModel { Id = 3, MotorId = 3, SteuerzeitSZ = 122.0 },
                new UeberstroemerModel { Id = 4, MotorId = 4 },
                new UeberstroemerModel { Id = 5, MotorId = 5, SteuerzeitSZ = 117.0 },
                new UeberstroemerModel { Id = 6, MotorId = 6 });

            #endregion Data-Seeding
        }
    }
}