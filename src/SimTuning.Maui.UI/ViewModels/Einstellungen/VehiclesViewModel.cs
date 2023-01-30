// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SimTuning.Core.Models;
using SimTuning.Core.Services;using SimTuning.Maui.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SimTuning.Core;

namespace SimTuning.Maui.UI.ViewModels.Einstellungen
{
    public class VehiclesViewModel : ViewModelBase
    {
        public VehiclesViewModel(
            ILogger<VehiclesViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._navigationService = navigationService ;
            this._vehicleService = vehicleService;
            //this._messenger = messenger;

            this.AreaQuantityUnits = new AreaQuantity();
            this.VolumeQuantityUnits = new VolumeQuantity();
            this.LengthQuantityUnits = new LengthQuantity();
            this.MassQuantityUnits = new MassQuantity();
            this.SpeedQuantityUnits = new SpeedQuantity();

            this.NewVehicleCommand = new RelayCommand(this.NewVehicle);
            this.DeleteVehicleCommand = new RelayCommand(this.DeleteVehicle);
            this.SaveVehicleCommand = new RelayCommand(this.SaveVehicle);

            this.ReloadData();
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
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Creates new vehicle.
        /// </summary>
        protected void NewVehicle()
        {
            try
            {
                // Vordefinieren des neuen Fahrzeugs
                this.Vehicles.Add(new Data.Models.VehiclesModel()
                {
                    Name = "Neues Fahrzeug",
                    Beschreibung = "Erstellt am " + DateTime.Now + " über Fahrzeug-Modul",
                    Deletable = true,
                });

                this.Vehicle = this.Vehicles.Last();

                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
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
            try
            {
                if (this.Vehicle.Id != null)
                {
                    _vehicleService.UpdateOne(this.Vehicle);
                }
                else
                {
                    this.Vehicle = _vehicleService.CreateOne(this.Vehicle);
                }

                this._saveButton = false;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Reloads the data.
        /// </summary>
        private void ReloadData()
        {
            this.Vehicles = new ObservableCollection<Data.Models.VehiclesModel>(_vehicleService.RetrieveVehicles());

            this.Engines = new ObservableCollection<Data.Models.MotorModel>(_vehicleService.RetrieveMotoren());
        }

        #endregion Methods

        #region Values

        
        protected readonly INavigationService _navigationService;
        protected readonly IVehicleService _vehicleService;

        /// <summary>
        /// Gets or sets a value indicating whether [save button].
        /// </summary>
        /// <value><c>true</c> if [save button]; otherwise, <c>false</c>.</value>
        protected bool _saveButton;

        private readonly ILogger<VehiclesViewModel> _logger;
        private Data.Models.MotorModel _engine;

        private ObservableCollection<Data.Models.MotorModel> _engines;
        private Data.Models.VehiclesModel _vehicle;

        private ObservableCollection<Data.Models.VehiclesModel> _vehicles;

        public ObservableCollection<UnitListItem> AreaQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the delete vehicle command.
        /// </summary>
        /// <value>The delete vehicle command.</value>
        public IRelayCommand DeleteVehicleCommand { get; set; }

        /// <summary>
        /// Gets or sets the engine.
        /// </summary>
        /// <value>The engine.</value>
        public Data.Models.MotorModel Engine
        {
            get => this._engine;
            set
            {
                if (this.Vehicle != null)
                {
                    // wenn Vehicle geladen wird; motor setzen für dropdown
                    if (this.Vehicle.MotorId.HasValue)
                    {
                        value = this.Engines.Where(m => m.Id == this.Vehicle.MotorId.Value).First();
                    }

                    if (value != null)
                    {
                        if (value.Id != this.Vehicle.MotorId)
                        {
                            // wenn beim Vehicle ein neuer Motor ausgewählt wird
                            this.Vehicle.Motor = value;
                        }
                    }
                }

                this.SetProperty(ref this._engine, value);

                // Motor-Werte für UI updaten
                //this.RaiseAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Gets or sets the engines.
        /// </summary>
        /// <value>The engines.</value>
        public ObservableCollection<Data.Models.MotorModel> Engines
        {
            get => this._engines;
            set => SetProperty(ref this._engines, value);
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
        /// Creates new vehiclecommand.
        /// </summary>
        /// <value>The new vehicle command.</value>
        public IRelayCommand NewVehicleCommand { get; set; }

        /// <summary>
        /// Gets or sets the save vehicle command.
        /// </summary>
        /// <value>The save vehicle command.</value>
        public IRelayCommand SaveVehicleCommand { get; set; }

        /// <summary>
        /// Gets the speed quantity units.
        /// </summary>
        /// <value>The speed quantity units.</value>
        public ObservableCollection<UnitListItem> SpeedQuantityUnits { get; }

        /// <summary>
        /// Gets or sets the vehicle.
        /// </summary>
        /// <value>The vehicle.</value>
        public Data.Models.VehiclesModel Vehicle
        {
            get => this._vehicle;
            set
            {
                if (value != null)
                {
                    // Laden des kompletten Datensatzes
                    _vehicleService.RetrieveOne(value.Id);
                }
                else
                {
                    // gerade gelöscht => letztes Vehicle neu laden
                    if (this.Vehicles.Count != 0)
                    {
                        this.Vehicle = this.Vehicles.Last();
                    }
                }

                // Einfügen
                this.SetProperty(ref this._vehicle, value);

                // nix mehr zu speichern
                this._saveButton = false;
                SaveVehicleCommand.NotifyCanExecuteChanged();

                // Motor refreshen
                this.Engine = null;

                //this.RaiseAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle beschreibung.
        /// </summary>
        /// <value>The vehicle beschreibung.</value>
        public string VehicleBeschreibung
        {
            get => this.Vehicle?.Beschreibung;
            set
            {
                if (this.Vehicle == null)
                {
                    return;
                }

                this.Vehicle.Beschreibung = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle cw.
        /// </summary>
        /// <value>The vehicle cw.</value>
        // public double? VehicleCw { get => this.Vehicle?.Cw; set { if (this.Vehicle ==
        // null) { return; }

        // this.Vehicle.Cw = value; this._saveButton = true;
        // SaveVehicleCommand.NotifyCanExecuteChanged(); } }

        /// <summary>
        /// Gets the vehicle dyno audio.
        /// </summary>
        /// <value>The vehicle dyno audio.</value>
        public ObservableCollection<Data.Models.DrehzahlModel> VehicleDynoAudio
        {
            get => this.Vehicle?.Dyno?.Drehzahl == null ? new ObservableCollection<Data.Models.DrehzahlModel>() : new ObservableCollection<Data.Models.DrehzahlModel>(this.Vehicle.Dyno.Drehzahl);
        }

        /// <summary>
        /// Gets or sets the vehicle dyno beschreibung.
        /// </summary>
        /// <value>The vehicle dyno beschreibung.</value>
        public string VehicleDynoBeschreibung
        {
            get => this.Vehicle?.Dyno?.Beschreibung;
            set
            {
                if (this.Vehicle?.Dyno == null)
                {
                    return;
                }

                this.Vehicle.Dyno.Beschreibung = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets the vehicle dyno dyno nm.
        /// </summary>
        /// <value>The vehicle dyno dyno nm.</value>
        // public ObservableCollection<Data.Models.DynoNmModel> VehicleDynoDynoNm { get =>
        // this.Vehicle?.Dyno?.DynoNm == null ? new
        // ObservableCollection<Data.Models.DynoNmModel>() : new
        // ObservableCollection<Data.Models.DynoNmModel>(this.Vehicle?.Dyno?.DynoNm); }

        /// <summary>
        /// Gets the vehicle dyno dyno ps.
        /// </summary>
        /// <value>The vehicle dyno dyno ps.</value>
        public ObservableCollection<Data.Models.DynoPsModel> VehicleDynoDynoPS
        {
            get => this.Vehicle?.Dyno?.DynoPS == null ? new ObservableCollection<Data.Models.DynoPsModel>() : new ObservableCollection<Data.Models.DynoPsModel>(this.Vehicle?.Dyno?.DynoPS);
        }

        /// <summary>
        /// Gets or sets the name of the vehicle dyno.
        /// </summary>
        /// <value>The name of the vehicle dyno.</value>
        public string VehicleDynoName
        {
            get => this.Vehicle?.Dyno?.Name;
            set
            {
                if (this.Vehicle?.Dyno == null)
                {
                    return;
                }
                this.Vehicle.Dyno.Name = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle front a.
        /// </summary>
        /// <value>The vehicle front a.</value>
        // public double? VehicleFrontA { get => this.Vehicle?.FrontA; set { if
        // (this.Vehicle == null) { return; } this.Vehicle.FrontA = value;
        // this._saveButton
        // = true; } }

        /// <summary>
        /// Gets or sets the vehicle front a unit.
        /// </summary>
        /// <value>The vehicle front a unit.</value>
        // public UnitListItem VehicleFrontAUnit { get =>
        // this.AreaQuantityUnits.SingleOrDefault(x =>
        // x.UnitEnumValue.Equals(this.Vehicle?.FrontAUnit)); set { if (this.Vehicle ==
        // null) { return; }

        // this.Vehicle.FrontAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue;
        // this.OnPropertyChanged(nameof(VehicleFrontA); } }

        /// <summary>
        /// Gets or sets the vehicle gewicht.
        /// </summary>
        /// <value>The vehicle gewicht.</value>
        public double? VehicleGewicht
        {
            get => this.Vehicle?.Gewicht;
            set
            {
                if (this.Vehicle == null)
                {
                    return;
                }

                this.Vehicle.Gewicht = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle gewicht unit.
        /// </summary>
        /// <value>The vehicle gewicht unit.</value>
        public UnitListItem VehicleGewichtUnit
        {
            get => this.MassQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.GewichtUnit));
            set
            {
                if (this.Vehicle == null)
                {
                    return;
                }

                this.Vehicle.GewichtUnit = (UnitsNet.Units.MassUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleGewicht));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass breite b.
        /// </summary>
        /// <value>The vehicle motor auslass breite b.</value>
        public double? VehicleMotorAuslassBreiteB
        {
            get => this.Vehicle?.Motor?.Auslass?.BreiteB;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.BreiteB = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass breite b unit.
        /// </summary>
        /// <value>The vehicle motor auslass breite b unit.</value>
        public UnitListItem VehicleMotorAuslassBreiteBUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.BreiteBUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.BreiteBUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorAuslassBreiteB));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a.</value>
        public double? VehicleMotorAuslassFlaecheA
        {
            get => this.Vehicle?.Motor?.Auslass?.FlaecheA;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.FlaecheA = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a unit.</value>
        public UnitListItem VehicleMotorAuslassFlaecheAUnit
        {
            get => this.AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.FlaecheAUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.FlaecheAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorAuslassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass hoehe h.
        /// </summary>
        /// <value>The vehicle motor auslass hoehe h.</value>
        public double? VehicleMotorAuslassHoeheH
        {
            get => this.Vehicle?.Motor?.Auslass?.HoeheH;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.HoeheH = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor auslass hoehe h unit.</value>
        public UnitListItem VehicleMotorAuslassHoeheHUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.HoeheHUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.HoeheHUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorAuslassHoeheH));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l.</value>
        public double? VehicleMotorAuslassLaengeL
        {
            get => this.Vehicle?.Motor?.Auslass?.LaengeL;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.LaengeL = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l unit.</value>
        public UnitListItem VehicleMotorAuslassLaengeLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Auslass?.LaengeLUnit));
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.LaengeLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorAuslassLaengeL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor auslass steuerzeit sz.</value>
        public double? VehicleMotorAuslassSteuerzeitSZ
        {
            get => this.Vehicle?.Motor?.Auslass?.SteuerzeitSZ;
            set
            {
                if (this.Vehicle?.Motor?.Auslass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Auslass.SteuerzeitSZ = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d.
        /// </summary>
        /// <value>The vehicle motor bohrung d.</value>
        public double? VehicleMotorBohrungD
        {
            get => this.Vehicle?.Motor?.BohrungD;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.BohrungD = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d unit.
        /// </summary>
        /// <value>The vehicle motor bohrung d unit.</value>
        public UnitListItem VehicleMotorBohrungDUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.BohrungDUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.BohrungDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorBohrungD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v.
        /// </summary>
        /// <value>The vehicle motor brennraum v.</value>
        public double? VehicleMotorBrennraumV
        {
            get => this.Vehicle?.Motor?.BrennraumV;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.BrennraumV = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v unit.
        /// </summary>
        /// <value>The vehicle motor brennraum v unit.</value>
        public UnitListItem VehicleMotorBrennraumVUnit
        {
            get => this.VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.BrennraumVUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.BrennraumVUnit = (UnitsNet.Units.VolumeUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorBrennraumV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor deachsierung l.
        /// </summary>
        /// <value>The vehicle motor deachsierung l.</value>
        public double? VehicleMotorDeachsierungL
        {
            get => this.Vehicle?.Motor?.DeachsierungL;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.DeachsierungL = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor deachsierung l unit.
        /// </summary>
        /// <value>The vehicle motor deachsierung l unit.</value>
        public UnitListItem VehicleMotorDeachsierungLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.DeachsierungLUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.DeachsierungLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorDeachsierungL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass breite b.
        /// </summary>
        /// <value>The vehicle motor einlass breite b.</value>
        public double? VehicleMotorEinlassBreiteB
        {
            get => this.Vehicle?.Motor?.Einlass?.BreiteB;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.BreiteB = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass breite b unit.
        /// </summary>
        /// <value>The vehicle motor einlass breite b unit.</value>
        public UnitListItem VehicleMotorEinlassBreiteBUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.BreiteBUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.BreiteBUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorEinlassBreiteB));
            }
        }

        public double? VehicleMotorEinlassDurchmesserD
        {
            get => this.Vehicle?.Motor?.Einlass?.DurchmesserD;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.DurchmesserD = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        public UnitListItem VehicleMotorEinlassDurchmesserDUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.DurchmesserDUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.DurchmesserDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorEinlassDurchmesserD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass flaeche a.
        /// </summary>
        /// <value>The vehicle motor einlass flaeche a.</value>
        public double? VehicleMotorEinlassFlaecheA
        {
            get => this.Vehicle?.Motor?.Einlass?.FlaecheA;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.FlaecheA = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor einlass flaeche a unit.</value>
        public UnitListItem VehicleMotorEinlassFlaecheAUnit
        {
            get => this.AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.FlaecheAUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.FlaecheAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorEinlassFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass hoehe h.
        /// </summary>
        /// <value>The vehicle motor einlass hoehe h.</value>
        public double? VehicleMotorEinlassHoeheH
        {
            get => this.Vehicle?.Motor?.Einlass?.HoeheH;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.HoeheH = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor einlass hoehe h unit.</value>
        public UnitListItem VehicleMotorEinlassHoeheHUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.HoeheHUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.HoeheHUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorEinlassHoeheH));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass laenge l.
        /// </summary>
        /// <value>The vehicle motor einlass laenge l.</value>
        public double? VehicleMotorEinlassLaengeL
        {
            get => this.Vehicle?.Motor?.Einlass?.LaengeL;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.LaengeL = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor einlass laenge l unit.</value>
        public UnitListItem VehicleMotorEinlassLaengeLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.LaengeLUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.LaengeLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorEinlassLaengeL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass luft bedarf.
        /// </summary>
        /// <value>The vehicle motor einlass luft bedarf.</value>
        public double? VehicleMotorEinlassLuftBedarf
        {
            get => this.Vehicle?.Motor?.Einlass?.LuftBedarf;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.LuftBedarf = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor einlass steuerzeit sz.</value>
        public double? VehicleMotorEinlassSteuerzeitSZ
        {
            get => this.Vehicle?.Motor?.Einlass?.SteuerzeitSZ;
            set
            {
                if (this.Vehicle?.Motor?.Einlass == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.SteuerzeitSZ = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser benzin luft f.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser benzin luft f.</value>
        public double? VehicleMotorEinlassVergaserBenzinLuftF
        {
            get => this.Vehicle?.Motor?.Einlass?.Vergaser?.BenzinLuftF;
            set
            {
                if (this.Vehicle?.Motor?.Einlass?.Vergaser == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.Vergaser.BenzinLuftF = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser durchmesser d.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser durchmesser d.</value>
        public double? VehicleMotorEinlassVergaserDurchmesserD
        {
            get => this.Vehicle?.Motor?.Einlass?.Vergaser?.DurchmesserD;
            set
            {
                if (this.Vehicle?.Motor?.Einlass?.Vergaser == null)
                {
                    return;
                }
                this.Vehicle.Motor.Einlass.Vergaser.DurchmesserD = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser durchmesser d unit.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser durchmesser d unit.</value>
        public UnitListItem VehicleMotorEinlassVergaserDurchmesserDUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Einlass?.Vergaser?.DurchmesserDUnit));
            set
            {
                if (this.Vehicle?.Motor?.Einlass?.Vergaser == null)
                {
                    return;
                }

                this.Vehicle.Motor.Einlass.Vergaser.DurchmesserDUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorEinlassVergaserDurchmesserD));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor heizwert u.
        /// </summary>
        /// <value>The vehicle motor heizwert u.</value>
        public double? VehicleMotorHeizwertU
        {
            get => this.Vehicle?.Motor?.HeizwertU;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.HeizwertU = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hub l.
        /// </summary>
        /// <value>The vehicle motor hub l.</value>
        public double? VehicleMotorHubL
        {
            get => this.Vehicle?.Motor?.HubL;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.HubL = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hub l unit.
        /// </summary>
        /// <value>The vehicle motor hub l unit.</value>
        public UnitListItem VehicleMotorHubLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.HubLUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }

                this.Vehicle.Motor.HubLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(this.VehicleMotorHubL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v.
        /// </summary>
        /// <value>The vehicle motor hubraum v.</value>
        public double? VehicleMotorHubraumV
        {
            get => this.Vehicle?.Motor?.HubraumV;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.HubraumV = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v unit.
        /// </summary>
        /// <value>The vehicle motor hubraum v unit.</value>
        public UnitListItem VehicleMotorHubraumVUnit
        {
            get => this.VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.HubraumVUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.HubraumVUnit = (UnitsNet.Units.VolumeUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorHubraumV));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kolben g.
        /// </summary>
        /// <value>The vehicle motor kolben g.</value>
        public double? VehicleMotorKolbenG
        {
            get => this.Vehicle?.Motor?.KolbenG;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.KolbenG = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kolben g unit.
        /// </summary>
        /// <value>The vehicle motor kolben g unit.</value>
        public UnitListItem VehicleMotorKolbenGUnit
        {
            get => this.SpeedQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.KolbenGUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.KolbenGUnit = (UnitsNet.Units.SpeedUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorKolbenG));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kurbelgehaeuse v.
        /// </summary>
        /// <value>The vehicle motor kurbelgehaeuse v.</value>
        public double? VehicleMotorKurbelgehaeuseV
        {
            get => this.Vehicle?.Motor?.KurbelgehaeuseV;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.KurbelgehaeuseV = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor kurbelgehaeuse v unit.
        /// </summary>
        /// <value>The vehicle motor kurbelgehaeuse v unit.</value>
        public UnitListItem VehicleMotorKurbelgehaeuseVUnit
        {
            get => this.VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.KurbelgehaeuseVUnit));

            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.KurbelgehaeuseVUnit = (UnitsNet.Units.VolumeUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorKurbelgehaeuseV));
            }
        }

        /// <summary>
        /// Gets or sets the name of the vehicle motor.
        /// </summary>
        /// <value>The name of the vehicle motor.</value>
        public string VehicleMotorName
        {
            get => this.Vehicle?.Motor?.Name;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.Name = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor pleul l.
        /// </summary>
        /// <value>The vehicle motor pleul l.</value>
        public double? VehicleMotorPleulL
        {
            get => this.Vehicle?.Motor?.PleulL;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.PleulL = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor pleul l unit.
        /// </summary>
        /// <value>The vehicle motor pleul l unit.</value>
        public UnitListItem VehicleMotorPleulLUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.PleulLUnit));
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.PleulLUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorPleulL));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor resonanz u.
        /// </summary>
        /// <value>The vehicle motor resonanz u.</value>
        public double? VehicleMotorResonanzU
        {
            get => this.Vehicle?.Motor?.ResonanzU;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.ResonanzU = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer anzahl.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer anzahl.</value>
        public double? VehicleMotorUeberstroemerAnzahl
        {
            get => this.Vehicle?.Motor?.Ueberstroemer?.Anzahl;
            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.Anzahl = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer breite b.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer breite b.</value>
        public double? VehicleMotorUeberstroemerBreiteB
        {
            get => this.Vehicle?.Motor?.Ueberstroemer?.BreiteB;
            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.BreiteB = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer breite b unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer breite b unit.</value>
        public UnitListItem VehicleMotorUeberstroemerBreiteBUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Ueberstroemer?.BreiteBUnit));

            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.BreiteBUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorUeberstroemerBreiteB));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer flaeche a.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer flaeche a.</value>
        public double? VehicleMotorUeberstroemerFlaecheA
        {
            get => this.Vehicle?.Motor?.Ueberstroemer?.FlaecheA;
            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.FlaecheA = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer flaeche a unit.</value>
        public UnitListItem VehicleMotorUeberstroemerFlaecheAUnit
        {
            get => this.AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Ueberstroemer?.FlaecheAUnit));
            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.FlaecheAUnit = (UnitsNet.Units.AreaUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorUeberstroemerFlaecheA));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h.</value>
        public double? VehicleMotorUeberstroemerHoeheH
        {
            get => this.Vehicle?.Motor?.Ueberstroemer?.HoeheH;
            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.HoeheH = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h unit.</value>
        public UnitListItem VehicleMotorUeberstroemerHoeheHUnit
        {
            get => this.LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(this.Vehicle?.Motor?.Ueberstroemer?.HoeheHUnit));
            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.HoeheHUnit = (UnitsNet.Units.LengthUnit)value?.UnitEnumValue;
                this.OnPropertyChanged(nameof(VehicleMotorUeberstroemerHoeheH));
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer steuerzeit sz.</value>
        public double? VehicleMotorUeberstroemerSteuerzeitSZ
        {
            get => this.Vehicle?.Motor?.Ueberstroemer?.SteuerzeitSZ;
            set
            {
                if (this.Vehicle?.Motor?.Ueberstroemer == null)
                {
                    return;
                }
                this.Vehicle.Motor.Ueberstroemer.SteuerzeitSZ = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor verdichtung v.
        /// </summary>
        /// <value>The vehicle motor verdichtung v.</value>
        public double? VehicleMotorVerdichtungV
        {
            get => this.Vehicle?.Motor?.VerdichtungV;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.VerdichtungV = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor zuendzeitpunkt.
        /// </summary>
        /// <value>The vehicle motor zuendzeitpunkt.</value>
        public double? VehicleMotorZuendzeitpunkt
        {
            get => this.Vehicle?.Motor?.Zuendzeitpunkt;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.Zuendzeitpunkt = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicle motor zylinder anz.
        /// </summary>
        /// <value>The vehicle motor zylinder anz.</value>
        public double? VehicleMotorZylinderAnz
        {
            get => this.Vehicle?.Motor?.ZylinderAnz;
            set
            {
                if (this.Vehicle?.Motor == null)
                {
                    return;
                }
                this.Vehicle.Motor.ZylinderAnz = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the name of the vehicle.
        /// </summary>
        /// <value>The name of the vehicle.</value>
        public string VehicleName
        {
            get => this.Vehicle?.Name;
            set
            {
                if (this.Vehicle == null)
                {
                    return;
                }
                this.Vehicle.Name = value;
                this._saveButton = true;
                SaveVehicleCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the vehicles.
        /// </summary>
        /// <value>The vehicles.</value>
        public ObservableCollection<Data.Models.VehiclesModel> Vehicles
        {
            get => this._vehicles;
            set => this.SetProperty(ref this._vehicles, value);
        }

        /// <summary>
        /// Gets or sets the vehicle uebersetzung.
        /// </summary>
        /// <value>The vehicle uebersetzung.</value>
        // public double? VehicleUebersetzung { get => this.Vehicle?.ueb; set { if
        // (this.Vehicle == null) { return; } this.Vehicle.Uebersetzung = value;
        // this._saveButton = true;
        //SaveVehicleCommand.NotifyCanExecuteChanged(); } }

        /// <summary>
        /// Gets the volume quantity units.
        /// </summary>
        /// <value>The volume quantity units.</value>
        public ObservableCollection<UnitListItem> VolumeQuantityUnits { get; }

        #endregion Values
    }
}