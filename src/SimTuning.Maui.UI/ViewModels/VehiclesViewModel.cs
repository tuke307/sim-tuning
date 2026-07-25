// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Models;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using System.Collections.ObjectModel;

namespace SimTuning.Maui.UI.ViewModels
{
    public class VehiclesViewModelBase : ViewModelBase
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
            set
            {
                if (Vehicle == null)
                {
                    return;
                }

                Vehicle.Beschreibung = value;
            }
        }

        /// <summary>
        /// Gets the vehicle dyno audio.
        /// </summary>
        /// <value>The vehicle dyno audio.</value>
        public ObservableCollection<Data.Models.DrehzahlModel> VehicleDynoAudio
        {
            get => Vehicle?.Dyno?.Drehzahl is ObservableCollection<Data.Models.DrehzahlModel> drehzahl ? new ObservableCollection<Data.Models.DrehzahlModel>(drehzahl) : new ObservableCollection<Data.Models.DrehzahlModel>();
        }

        /// <summary>
        /// Gets or sets the vehicle dyno beschreibung.
        /// </summary>
        /// <value>The vehicle dyno beschreibung.</value>
        public string? VehicleDynoBeschreibung
        {
            get => Vehicle?.Dyno?.Beschreibung;
            set
            {
                if (Vehicle?.Dyno == null)
                {
                    return;
                }

                Vehicle.Dyno.Beschreibung = value;
            }
        }

        /// <summary>
        /// Gets the vehicle dyno dyno ps.
        /// </summary>
        /// <value>The vehicle dyno dyno ps.</value>
        public ObservableCollection<Data.Models.DynoPsModel> VehicleDynoDynoPS
        {
            get => Vehicle?.Dyno?.DynoPS is ObservableCollection<Data.Models.DynoPsModel> dynoPS ? new ObservableCollection<Data.Models.DynoPsModel>(dynoPS) : new ObservableCollection<Data.Models.DynoPsModel>();
        }

        /// <summary>
        /// Gets or sets the name of the vehicle dyno.
        /// </summary>
        /// <value>The name of the vehicle dyno.</value>
        public string? VehicleDynoName
        {
            get => Vehicle?.Dyno?.Name;
            set
            {
                if (Vehicle?.Dyno == null)
                {
                    return;
                }
                Vehicle.Dyno.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle gewicht.
        /// </summary>
        /// <value>The vehicle gewicht.</value>
        public double? VehicleGewicht
        {
            get => Vehicle?.Gewicht;
            set
            {
                if (Vehicle == null)
                {
                    return;
                }

                Vehicle.Gewicht = value;
            }
        }

        // Vehicle.FrontAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue; OnPropertyChanged(nameof(VehicleFrontA); } }
        /// <summary>
        /// Gets or sets the vehicle gewicht unit.
        /// </summary>
        /// <value>The vehicle gewicht unit.</value>
        public UnitListItem? VehicleGewichtUnit
        {
            get => MassQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.GewichtUnit));
            set
            {
                if (Vehicle == null)
                {
                    return;
                }

                Vehicle.GewichtUnit = (UnitsNet.Units.MassUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleGewicht));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle front a unit.
        /// </summary>
        /// <value>The vehicle front a unit.</value>
        // public UnitListItem VehicleFrontAUnit { get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.FrontAUnit)); set { if (Vehicle ==
        // null) { return; }
        /// <summary>
        /// Gets or sets the vehicle motor auslass breite b.
        /// </summary>
        /// <value>The vehicle motor auslass breite b.</value>
        public double? VehicleMotorAuslassBreiteB
        {
            get => Vehicle?.Motor?.Auslass?.BreiteB;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.BreiteB = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle front a.
        /// </summary>
        /// <value>The vehicle front a.</value>
        // public double? VehicleFrontA { get => Vehicle?.FrontA; set { if (Vehicle == null) { return; } Vehicle.FrontA = value; _saveButton
        // = true; } }
        /// <summary>
        /// Gets or sets the vehicle motor auslass breite b unit.
        /// </summary>
        /// <value>The vehicle motor auslass breite b unit.</value>
        public UnitListItem? VehicleMotorAuslassBreiteBUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.BreiteBUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.BreiteBUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorAuslassBreiteB));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a.</value>
        public double? VehicleMotorAuslassFlaecheA
        {
            get => Vehicle?.Motor?.Auslass?.FlaecheA;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.FlaecheA = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a unit.</value>
        public UnitListItem? VehicleMotorAuslassFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.FlaecheAUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.FlaecheAUnit = ((UnitsNet.Units.AreaUnit?)value?.UnitEnumValue).GetValueOrDefault();
                OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass hoehe h.
        /// </summary>
        /// <value>The vehicle motor auslass hoehe h.</value>
        public double? VehicleMotorAuslassHoeheH
        {
            get => Vehicle?.Motor?.Auslass?.HoeheH;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.HoeheH = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor auslass hoehe h unit.</value>
        public UnitListItem? VehicleMotorAuslassHoeheHUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.HoeheHUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.HoeheHUnit = ((UnitsNet.Units.LengthUnit?)value?.UnitEnumValue).GetValueOrDefault();
                OnPropertyChanged(nameof(VehicleMotorAuslassHoeheH));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l.</value>
        public double? VehicleMotorAuslassLaengeL
        {
            get => Vehicle?.Motor?.Auslass?.LaengeL;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.LaengeL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l unit.</value>
        public UnitListItem? VehicleMotorAuslassLaengeLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.LaengeLUnit));
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.LaengeLUnit = ((UnitsNet.Units.LengthUnit?)value?.UnitEnumValue).GetValueOrDefault();
                OnPropertyChanged(nameof(VehicleMotorAuslassLaengeL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor auslass steuerzeit sz.</value>
        public double? VehicleMotorAuslassSteuerzeitSZ
        {
            get => Vehicle?.Motor?.Auslass?.SteuerzeitSZ;
            set
            {
                if (Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                Vehicle.Motor.Auslass.SteuerzeitSZ = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d.
        /// </summary>
        /// <value>The vehicle motor bohrung d.</value>
        public double? VehicleMotorBohrungD
        {
            get => Vehicle?.Motor?.BohrungD;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.BohrungD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d unit.
        /// </summary>
        /// <value>The vehicle motor bohrung d unit.</value>
        public UnitListItem? VehicleMotorBohrungDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.BohrungDUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.BohrungDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorBohrungD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v.
        /// </summary>
        /// <value>The vehicle motor brennraum v.</value>
        public double? VehicleMotorBrennraumV
        {
            get => Vehicle?.Motor?.BrennraumV;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.BrennraumV = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v unit.
        /// </summary>
        /// <value>The vehicle motor brennraum v unit.</value>
        public UnitListItem? VehicleMotorBrennraumVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.BrennraumVUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.BrennraumVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorBrennraumV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor deachsierung l.
        /// </summary>
        /// <value>The vehicle motor deachsierung l.</value>
        public double? VehicleMotorDeachsierungL
        {
            get => Vehicle?.Motor?.DeachsierungL;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.DeachsierungL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor deachsierung l unit.
        /// </summary>
        /// <value>The vehicle motor deachsierung l unit.</value>
        public UnitListItem? VehicleMotorDeachsierungLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.DeachsierungLUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.DeachsierungLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorDeachsierungL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass breite b.
        /// </summary>
        /// <value>The vehicle motor einlass breite b.</value>
        public double? VehicleMotorEinlassBreiteB
        {
            get => Vehicle?.Motor?.Einlass?.BreiteB;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.BreiteB = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass breite b unit.
        /// </summary>
        /// <value>The vehicle motor einlass breite b unit.</value>
        public UnitListItem? VehicleMotorEinlassBreiteBUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.BreiteBUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.BreiteBUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassBreiteB));
            }
        }

        public double? VehicleMotorEinlassDurchmesserD
        {
            get => Vehicle?.Motor?.Einlass?.DurchmesserD;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.DurchmesserD = value;
            }
        }

        public UnitListItem? VehicleMotorEinlassDurchmesserDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.DurchmesserDUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.DurchmesserDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassDurchmesserD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass flaeche a.
        /// </summary>
        /// <value>The vehicle motor einlass flaeche a.</value>
        public double? VehicleMotorEinlassFlaecheA
        {
            get => Vehicle?.Motor?.Einlass?.FlaecheA;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.FlaecheA = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor einlass flaeche a unit.</value>
        public UnitListItem? VehicleMotorEinlassFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.FlaecheAUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.FlaecheAUnit = (UnitsNet.Units.AreaUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass hoehe h.
        /// </summary>
        /// <value>The vehicle motor einlass hoehe h.</value>
        public double? VehicleMotorEinlassHoeheH
        {
            get => Vehicle?.Motor?.Einlass?.HoeheH;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.HoeheH = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor einlass hoehe h unit.</value>
        public UnitListItem? VehicleMotorEinlassHoeheHUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.HoeheHUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.HoeheHUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassHoeheH));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass laenge l.
        /// </summary>
        /// <value>The vehicle motor einlass laenge l.</value>
        public double? VehicleMotorEinlassLaengeL
        {
            get => Vehicle?.Motor?.Einlass?.LaengeL;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.LaengeL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor einlass laenge l unit.</value>
        public UnitListItem? VehicleMotorEinlassLaengeLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.LaengeLUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.LaengeLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassLaengeL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass luft bedarf.
        /// </summary>
        /// <value>The vehicle motor einlass luft bedarf.</value>
        public double? VehicleMotorEinlassLuftBedarf
        {
            get => Vehicle?.Motor?.Einlass?.LuftBedarf;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.LuftBedarf = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor einlass steuerzeit sz.</value>
        public double? VehicleMotorEinlassSteuerzeitSZ
        {
            get => Vehicle?.Motor?.Einlass?.SteuerzeitSZ;
            set
            {
                if (Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.SteuerzeitSZ = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser benzin luft f.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser benzin luft f.</value>
        public double? VehicleMotorEinlassVergaserBenzinLuftF
        {
            get => Vehicle?.Motor?.Einlass?.Vergaser?.BenzinLuftF;
            set
            {
                if (Vehicle?.Motor?.Einlass?.Vergaser == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.Vergaser.BenzinLuftF = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser durchmesser d.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser durchmesser d.</value>
        public double? VehicleMotorEinlassVergaserDurchmesserD
        {
            get => Vehicle?.Motor?.Einlass?.Vergaser?.DurchmesserD;
            set
            {
                if (Vehicle?.Motor?.Einlass?.Vergaser == null)
                {
                    return;
                }
                Vehicle.Motor.Einlass.Vergaser.DurchmesserD = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser durchmesser d unit.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser durchmesser d unit.</value>
        public UnitListItem? VehicleMotorEinlassVergaserDurchmesserDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.Vergaser?.DurchmesserDUnit));
            set
            {
                if (Vehicle?.Motor?.Einlass?.Vergaser == null)
                {
                    return;
                }

                Vehicle.Motor.Einlass.Vergaser.DurchmesserDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorEinlassVergaserDurchmesserD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor heizwert u.
        /// </summary>
        /// <value>The vehicle motor heizwert u.</value>
        public double? VehicleMotorHeizwertU
        {
            get => Vehicle?.Motor?.HeizwertU;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.HeizwertU = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hub l.
        /// </summary>
        /// <value>The vehicle motor hub l.</value>
        public double? VehicleMotorHubL
        {
            get => Vehicle?.Motor?.HubL;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.HubL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hub l unit.
        /// </summary>
        /// <value>The vehicle motor hub l unit.</value>
        public UnitListItem? VehicleMotorHubLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.HubLUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }

                Vehicle.Motor.HubLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorHubL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v.
        /// </summary>
        /// <value>The vehicle motor hubraum v.</value>
        public double? VehicleMotorHubraumV
        {
            get => Vehicle?.Motor?.HubraumV;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.HubraumV = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v unit.
        /// </summary>
        /// <value>The vehicle motor hubraum v unit.</value>
        public UnitListItem? VehicleMotorHubraumVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.HubraumVUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.HubraumVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorHubraumV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kolben g.
        /// </summary>
        /// <value>The vehicle motor kolben g.</value>
        public double? VehicleMotorKolbenG
        {
            get => Vehicle?.Motor?.KolbenG;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.KolbenG = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kolben g unit.
        /// </summary>
        /// <value>The vehicle motor kolben g unit.</value>
        public UnitListItem? VehicleMotorKolbenGUnit
        {
            get => SpeedQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.KolbenGUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.KolbenGUnit = (UnitsNet.Units.SpeedUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorKolbenG));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kurbelgehaeuse v.
        /// </summary>
        /// <value>The vehicle motor kurbelgehaeuse v.</value>
        public double? VehicleMotorKurbelgehaeuseV
        {
            get => Vehicle?.Motor?.KurbelgehaeuseV;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.KurbelgehaeuseV = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kurbelgehaeuse v unit.
        /// </summary>
        /// <value>The vehicle motor kurbelgehaeuse v unit.</value>
        public UnitListItem? VehicleMotorKurbelgehaeuseVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.KurbelgehaeuseVUnit));

            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.KurbelgehaeuseVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorKurbelgehaeuseV));
            }
        }

        /// <summary>
        /// Gets or sets the name of the vehicle motor.
        /// </summary>
        /// <value>The name of the vehicle motor.</value>
        public string? VehicleMotorName
        {
            get => Vehicle?.Motor?.Name;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor pleul l.
        /// </summary>
        /// <value>The vehicle motor pleul l.</value>
        public double? VehicleMotorPleulL
        {
            get => Vehicle?.Motor?.PleulL;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.PleulL = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor pleul l unit.
        /// </summary>
        /// <value>The vehicle motor pleul l unit.</value>
        public UnitListItem? VehicleMotorPleulLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.PleulLUnit));
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.PleulLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorPleulL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor resonanz u.
        /// </summary>
        /// <value>The vehicle motor resonanz u.</value>
        public double? VehicleMotorResonanzU
        {
            get => Vehicle?.Motor?.ResonanzU;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.ResonanzU = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer anzahl.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer anzahl.</value>
        public double? VehicleMotorUeberstroemerAnzahl
        {
            get => Vehicle?.Motor?.Ueberstroemer?.Anzahl;
            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.Anzahl = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer breite b.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer breite b.</value>
        public double? VehicleMotorUeberstroemerBreiteB
        {
            get => Vehicle?.Motor?.Ueberstroemer?.BreiteB;
            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.BreiteB = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer breite b unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer breite b unit.</value>
        public UnitListItem? VehicleMotorUeberstroemerBreiteBUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Ueberstroemer?.BreiteBUnit));

            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.BreiteBUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorUeberstroemerBreiteB));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer flaeche a.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer flaeche a.</value>
        public double? VehicleMotorUeberstroemerFlaecheA
        {
            get => Vehicle?.Motor?.Ueberstroemer?.FlaecheA;
            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.FlaecheA = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer flaeche a unit.</value>
        public UnitListItem? VehicleMotorUeberstroemerFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Ueberstroemer?.FlaecheAUnit));
            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.FlaecheAUnit = (UnitsNet.Units.AreaUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorUeberstroemerFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h.</value>
        public double? VehicleMotorUeberstroemerHoeheH
        {
            get => Vehicle?.Motor?.Ueberstroemer?.HoeheH;
            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.HoeheH = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h unit.</value>
        public UnitListItem? VehicleMotorUeberstroemerHoeheHUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Ueberstroemer?.HoeheHUnit));
            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.HoeheHUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue;
                OnPropertyChanged(nameof(VehicleMotorUeberstroemerHoeheH));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer steuerzeit sz.</value>
        public double? VehicleMotorUeberstroemerSteuerzeitSZ
        {
            get => Vehicle?.Motor?.Ueberstroemer?.SteuerzeitSZ;
            set
            {
                if (Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                Vehicle.Motor.Ueberstroemer.SteuerzeitSZ = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor verdichtung v.
        /// </summary>
        /// <value>The vehicle motor verdichtung v.</value>
        public double? VehicleMotorVerdichtungV
        {
            get => Vehicle?.Motor?.VerdichtungV;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.VerdichtungV = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor zuendzeitpunkt.
        /// </summary>
        /// <value>The vehicle motor zuendzeitpunkt.</value>
        public double? VehicleMotorZuendzeitpunkt
        {
            get => Vehicle?.Motor?.Zuendzeitpunkt;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.Zuendzeitpunkt = value;
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor zylinder anz.
        /// </summary>
        /// <value>The vehicle motor zylinder anz.</value>
        public double? VehicleMotorZylinderAnz
        {
            get => Vehicle?.Motor?.ZylinderAnz;
            set
            {
                if (Vehicle?.Motor == null)
                {
                    return;
                }
                Vehicle.Motor.ZylinderAnz = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the vehicle.
        /// </summary>
        /// <value>The name of the vehicle.</value>
        public string? VehicleName
        {
            get => Vehicle?.Name;
            set
            {
                if (Vehicle == null)
                {
                    return;
                }
                Vehicle.Name = value;
            }
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

        private void raiseAllPropertyChanged()
        {
            OnPropertyChanged(nameof(VehicleName));
            OnPropertyChanged(nameof(VehicleBeschreibung));

            OnPropertyChanged(nameof(VehicleGewicht));
            OnPropertyChanged(nameof(VehicleGewichtUnit));
            OnPropertyChanged(nameof(VehicleMotorName));
            OnPropertyChanged(nameof(VehicleMotorHubL));
            OnPropertyChanged(nameof(VehicleMotorHubLUnit));
            OnPropertyChanged(nameof(VehicleMotorHubL));
            OnPropertyChanged(nameof(VehicleMotorHubLUnit));
            OnPropertyChanged(nameof(VehicleMotorAuslassBreiteB));
            OnPropertyChanged(nameof(VehicleMotorAuslassBreiteBUnit));
            OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheA));
            OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheAUnit));
            OnPropertyChanged(nameof(VehicleMotorAuslassHoeheH));
            OnPropertyChanged(nameof(VehicleMotorAuslassHoeheHUnit));
            OnPropertyChanged(nameof(VehicleMotorAuslassLaengeL));
            OnPropertyChanged(nameof(VehicleMotorAuslassLaengeLUnit));
            OnPropertyChanged(nameof(VehicleMotorAuslassSteuerzeitSZ));
            OnPropertyChanged(nameof(VehicleMotorBohrungD));
            OnPropertyChanged(nameof(VehicleMotorBohrungDUnit));
            OnPropertyChanged(nameof(VehicleMotorBrennraumV));
            OnPropertyChanged(nameof(VehicleMotorBrennraumVUnit));
            OnPropertyChanged(nameof(VehicleMotorDeachsierungL﻿));
            OnPropertyChanged(nameof(VehicleMotorDeachsierungLUnit));
            OnPropertyChanged(nameof(VehicleMotorEinlassBreiteB));
            OnPropertyChanged(nameof(VehicleMotorEinlassBreiteBUnit));
            OnPropertyChanged(nameof(VehicleMotorEinlassDurchmesserD));
            OnPropertyChanged(nameof(VehicleMotorEinlassDurchmesserDUnit));
            OnPropertyChanged(nameof(VehicleMotorEinlassFlaecheA));
            OnPropertyChanged(nameof(VehicleMotorEinlassFlaecheAUnit));
            OnPropertyChanged(nameof(VehicleMotorEinlassHoeheH));
            OnPropertyChanged(nameof(VehicleMotorEinlassHoeheHUnit));
            OnPropertyChanged(nameof(VehicleMotorEinlassLaengeL));
            OnPropertyChanged(nameof(VehicleMotorEinlassLaengeLUnit));
            OnPropertyChanged(nameof(VehicleMotorEinlassLuftBedarf));
            OnPropertyChanged(nameof(VehicleMotorEinlassSteuerzeitSZ));
            OnPropertyChanged(nameof(VehicleMotorEinlassVergaserBenzinLuftF));
            OnPropertyChanged(nameof(VehicleMotorEinlassVergaserDurchmesserD));
            OnPropertyChanged(nameof(VehicleMotorEinlassVergaserDurchmesserDUnit));
            OnPropertyChanged(nameof(VehicleMotorHeizwertU));
            OnPropertyChanged(nameof(VehicleMotorHubL));
            OnPropertyChanged(nameof(VehicleMotorHubLUnit));
            OnPropertyChanged(nameof(VehicleMotorHubraumV));
            OnPropertyChanged(nameof(VehicleMotorHubraumVUnit));
            OnPropertyChanged(nameof(VehicleMotorKolbenG));
            OnPropertyChanged(nameof(VehicleMotorKolbenGUnit));
            OnPropertyChanged(nameof(VehicleMotorKurbelgehaeuseV));
            OnPropertyChanged(nameof(VehicleMotorKurbelgehaeuseVUnit));
            OnPropertyChanged(nameof(VehicleMotorName));
            OnPropertyChanged(nameof(VehicleMotorPleulL));
            OnPropertyChanged(nameof(VehicleMotorPleulLUnit));
            OnPropertyChanged(nameof(VehicleMotorResonanzU));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerAnzahl));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerBreiteB));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerBreiteBUnit));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerFlaecheA));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerFlaecheAUnit));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerHoeheH));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerHoeheHUnit));
            OnPropertyChanged(nameof(VehicleMotorUeberstroemerSteuerzeitSZ));
            OnPropertyChanged(nameof(VehicleMotorVerdichtungV));
            OnPropertyChanged(nameof(VehicleMotorZuendzeitpunkt));
            OnPropertyChanged(nameof(VehicleMotorZylinderAnz));

            OnPropertyChanged(nameof(VehicleDynoAudio));
            OnPropertyChanged(nameof(VehicleDynoAudio));
            OnPropertyChanged(nameof(VehicleDynoBeschreibung));
            OnPropertyChanged(nameof(VehicleDynoDynoPS));
            OnPropertyChanged(nameof(VehicleDynoName));
        }

        #endregion Values
    }
}