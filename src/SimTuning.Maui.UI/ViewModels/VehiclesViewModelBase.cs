// Copyright (c) 2025 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Models;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using System;
using System.Collections.ObjectModel;

namespace SimTuning.Maui.UI.ViewModels
{
    /// <summary>
    /// Basis-ViewModel für alle ViewModels, die ein Vehicle (und dessen Motor)
    /// bearbeiten. Hält die Auswahl-Listen, die Pflicht-Dienste und die
    /// "--&gt; Model"-Spiegel-Eigenschaften (siehe partielle Klassen
    /// <c>VehiclesViewModelBase.*.cs</c>).
    /// </summary>
    public partial class VehiclesViewModelBase : ViewModelBase
    {
        public VehiclesViewModelBase(
            ILogger<VehiclesViewModelBase> logger,
            IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;

            AreaQuantityUnits = new AreaQuantity();
            VolumeQuantityUnits = new VolumeQuantity();
            LengthQuantityUnits = new LengthQuantity();
            MassQuantityUnits = new MassQuantity();
            SpeedQuantityUnits = new SpeedQuantity();

            ReloadData();
        }

        #region Methods

        /// <summary>
        /// Reloads the data.
        /// </summary>
        private void ReloadData()
        {
            Vehicles = new ObservableCollection<Data.Models.VehiclesModel>(_vehicleService.RetrieveVehicles() ?? new List<Data.Models.VehiclesModel>());

            Engines = new ObservableCollection<Data.Models.MotorModel>(_vehicleService.RetrieveMotoren() ?? new List<Data.Models.MotorModel>());
        }

        /// <summary>
        /// Schreibt einen Wert/eine Einheit durch auf das dahinterliegende EF-Model
        /// einer Spiegel-Eigenschaft. Verhält sich identisch zum bisherigen
        /// handgeschriebenen Muster
        ///   if (Vehicle?.Motor?.… == null) return; Vehicle.Motor.….Feld = …;
        /// optional gefolgt von OnPropertyChanged(nameof(GespiegelterWert)).
        /// </summary>
        /// <typeparam name="TParent">Der EF-Knoten, der das Feld besitzt (Referenztyp).</typeparam>
        /// <param name="parent">Die bereits null-geprüfte Elternreferenz.</param>
        /// <param name="apply">Schreibt den Wert/die Einheit auf <paramref name="parent"/> inkl. etwaigem Cast.</param>
        /// <param name="raisedName">Falls gesetzt, wird diese Eigenschaft nach dem Schreiben geworfen (nur bei Einheiten).</param>
        protected void SetMirror<TParent>(TParent? parent, Action<TParent> apply, string? raisedName = null)
            where TParent : class
        {
            if (parent is null)
            {
                return;
            }

            apply(parent);

            if (raisedName is not null)
            {
                OnPropertyChanged(raisedName);
            }
        }

        /// <summary>
        /// Wirft PropertyChanged für alle Bindings. <see cref="ObservableObject"/>
        /// bzw. die MAUI-Binding-Engine interpretiert <see cref="string.Empty"/>
        /// als "alle Eigenschaften haben sich geändert" — jede Spiegel-Eigenschaft
        /// wird neu ausgewertet, sobald das Vehicle gewechselt wird.
        /// </summary>
        private void raiseAllPropertyChanged() => OnPropertyChanged(string.Empty);

        #endregion Methods

        #region Values

        protected readonly IVehicleService _vehicleService;

        private readonly ILogger<VehiclesViewModelBase> _logger;
        private Data.Models.MotorModel? _engine;

        private ObservableCollection<Data.Models.MotorModel> _engines = new ObservableCollection<Data.Models.MotorModel>();
        private Data.Models.VehiclesModel? _vehicle;

        private ObservableCollection<Data.Models.VehiclesModel> _vehicles = new ObservableCollection<Data.Models.VehiclesModel>();

        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the engine.
        /// </summary>
        /// <value>The engine.</value>
        public Data.Models.MotorModel? Engine
        {
            get => _engine;
            set
            {
                if (Vehicle != null)
                {
                    // wenn Vehicle geladen wird; motor setzen für dropdown
                    if (Vehicle.MotorId.HasValue)
                    {
                        value = Engines.Where(m => m.Id == Vehicle.MotorId.Value).First();
                    }

                    if (value != null)
                    {
                        if (value.Id != Vehicle.MotorId)
                        {
                            // wenn beim Vehicle ein neuer Motor ausgewählt wird
                            Vehicle.Motor = value;
                        }
                    }
                }

                SetProperty(ref _engine, value);

                // Motor-Werte für UI updaten
                raiseAllPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the engines.
        /// </summary>
        /// <value>The engines.</value>
        public ObservableCollection<Data.Models.MotorModel> Engines
        {
            get => _engines;
            set => SetProperty(ref _engines, value);
        }

        /// <summary>
        /// Gets the length quantity units.
        /// </summary>
        /// <value>The length quantity units.</value>
        public ObservableCollection<UnitListItem> LengthQuantityUnits { get; }

        /// <summary>
        /// Gets the mass quantity units.
        /// </summary>
        /// <value>The mass quantity units.</value>
        public ObservableCollection<UnitListItem> MassQuantityUnits { get; }

        /// <summary>
        /// Gets the speed quantity units.
        /// </summary>
        /// <value>The speed quantity units.</value>
        public ObservableCollection<UnitListItem> SpeedQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the vehicle.
        /// </summary>
        /// <value>The vehicle.</value>
        public Data.Models.VehiclesModel? Vehicle
        {
            get => _vehicle;
            set
            {
                if (value != null)
                {
                    // Laden des kompletten Datensatzes
                    _vehicleService.RetrieveOne(value.Id);
                }
                else
                {
                    // Just deleted => load last vehicle
                    if (Vehicles.Count > 0)
                    {
                        value = Vehicles.Last();
                    }
                }

                // Einfügen
                SetProperty(ref _vehicle, value);

                // Motor refreshen
                Engine = null;

                raiseAllPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle beschreibung.
        /// </summary>
        /// <value>The vehicle beschreibung.</value>
        public string? VehicleBeschreibung
        {
            get => Vehicle?.Beschreibung;
            set => SetMirror(Vehicle, v => v.Beschreibung = value);
        }

        /// <summary>
        /// Gets or sets the vehicle gewicht.
        /// </summary>
        /// <value>The vehicle gewicht.</value>
        public double? VehicleGewicht
        {
            get => Vehicle?.Gewicht;
            set => SetMirror(Vehicle, v => v.Gewicht = value);
        }

        /// <summary>
        /// Gets or sets the vehicle gewicht unit.
        /// </summary>
        /// <value>The vehicle gewicht unit.</value>
        public UnitListItem? VehicleGewichtUnit
        {
            get => MassQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.GewichtUnit));
            set => SetMirror(Vehicle, v => v.GewichtUnit = (UnitsNet.Units.MassUnit?)value?.UnitEnumValue, nameof(VehicleGewicht));
        }

        /// <summary>
        /// Gets or sets the name of the vehicle.
        /// </summary>
        /// <value>The name of the vehicle.</value>
        public string? VehicleName
        {
            get => Vehicle?.Name;
            set => SetMirror(Vehicle, v => v.Name = value);
        }

        /// <summary>
        /// Gets or sets the vehicles.
        /// </summary>
        /// <value>The vehicles.</value>
        public ObservableCollection<Data.Models.VehiclesModel> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        /// <summary>
        /// Gets the volume quantity units.
        /// </summary>
        /// <value>The volume quantity units.</value>
        public ObservableCollection<UnitListItem> VolumeQuantityUnits { get; }

        // public UnitListItem VehicleFrontAUnit { get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.FrontAUnit)); set { if (Vehicle ==
        // null) { return; }
        /// <summary>
        /// Gets or sets the vehicle front a.
        /// </summary>
        /// <value>The vehicle front a.</value>
        // public double? VehicleFrontA { get => Vehicle?.FrontA; set { if (Vehicle == null) { return; } Vehicle.FrontA = value; _saveButton
        // = true; } }

        #endregion Values
    }
}
