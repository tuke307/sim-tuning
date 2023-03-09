// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimTuning.Core;
using SimTuning.Core.Helpers;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using System.Collections.ObjectModel;
using System.IO.Compression;

namespace SimTuning.Maui.UI.ViewModels
{
    public class DynoDataViewModel : ViewModelBase
    {
        public DynoDataViewModel(
            ILogger<DynoDataViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService,
            IBrowserService browserService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
            this._vehicleService = vehicleService;
            this._browserService = browserService;

            this.NewDynoCommand = new RelayCommand(this.NewDyno);
            this.DeleteDynoCommand = new RelayCommand(this.DeleteDyno);
            this.SaveDynoCommand = new RelayCommand(this.SaveDyno);

            this.ImportDynoCommand = new AsyncRelayCommand(this.ImportDyno);

            //this.Runtime = new AsyncRelayCommand(async () => await _INavigationService.Navigate<Dyno.RuntimeViewModel>());

            this.ShowSaveButtonCommand = new RelayCommand(() => this.SaveButton = true);
            this.ExportDynoCommand = new AsyncRelayCommand(this.ExportDynoAsync);

            this.Dynos = new ObservableCollection<DynoModel>(_vehicleService.RetrieveDynos());
            this.Dyno = _vehicleService.RetrieveOneActive();
        }

        #region Methods

        /// <summary>
        /// Deletes the dyno.
        /// </summary>
        protected void DeleteDyno()
        {
            try
            {
                // in lokale liste aktualisieren
                Dynos.Remove(this.Dyno);
                OnPropertyChanged(nameof(Dynos));

                _vehicleService.DeleteOne(this.Dyno);

                // letzten in liste laden.
                this.Dyno = Dynos.Last();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Exports the dyno.
        /// </summary>
        protected async Task ExportDynoAsync()
        {
            try
            {
                // erstellen der json.
                // TODO: reference test check
                string json = JsonConvert.SerializeObject(this.Dyno, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    // ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                });

                File.WriteAllText(GeneralSettings.DataExportFilePath, json);

                // Dateien die gepackt werden sollen
                var list = new List<string>()
                {
                    GeneralSettings.DataExportFilePath,
                    GeneralSettings.AudioAccelerationFilePath,
                    GeneralSettings.AudioRolloutFilePath,
                };

                // ZIP archiv erstellen
                Functions.CreateZipFile(GeneralSettings.DataExportArchivePath, list);

                await Share.RequestAsync(new ShareFileRequest
                {
                    File = new ShareFile(GeneralSettings.DataExportArchivePath),
                }).ConfigureAwait(true);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Creates new dyno.
        /// </summary>
        protected void NewDyno()
        {
            try
            {
                // Fahrzeug zurücksetzen
                this.Vehicle = null;

                DynoModel dyno = new DynoModel()
                {
                    Name = "Dyno-Durchgang",
                    Beschreibung = $"Erstellt am {DateTime.Now}",
                };

                // gespeichert wird erst bei SaveDyno() erstmal lokal hinzufügen
                this.Dynos.Add(dyno);

                // vorauswahl
                this.Dyno = this.Dynos.Last();
                this.CreateNewVehicle = true;

                this.SaveButton = true;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Saves the dyno.
        /// </summary>
        /// <returns>Status.</returns>
        protected void SaveDyno()
        {
            if (this.Dyno == null)
            {
                this.SaveButton = false;
            }

            try
            {
                // Vehicle zuweisen
                if (this.TakeExistingVehicle)
                {
                    // kein Vehicle ausgewählt
                    if (this.Vehicle == null)
                    {
                        return;
                    }

                    // nur id zuweisen
                    this.Dyno.VehicleId = this.Vehicle.Id.Value;
                }

                // neues Fahrzeug erstellen
                else if (this.CreateNewVehicle)
                {
                    VehiclesModel vehicle = new VehiclesModel()
                    {
                        Name = "Dyno-Fahrzeug",
                        Beschreibung = "Erstellt über Dyno-Modul mit der Option 'Neues Fahrzeug erstellen'",
                        Deletable = true,
                    };

                    vehicle = _vehicleService.CreateOne(vehicle);

                    // Lokaler list auswählen
                    OnPropertyChanged(nameof(Vehicles));
                    this.Vehicle = this.Vehicles.Last();

                    // neues Vehicle Dyno zuweisen
                    this.Dyno.Vehicle = vehicle;

                    // Radio buttons zurücksetzen
                    this.CreateNewVehicle = false;
                    this.TakeExistingVehicle = true;
                }

                if (Dyno.Id == null)
                {
                    this.Dyno = _vehicleService.CreateOne(this.Dyno);
                }
                else
                {
                    _vehicleService.UpdateOne(this.Dyno);
                }

                // wenn bereits geladene seiten mit dyno daten Refresh aller Dyno-Datensätze
                //if (this._messenger.HasSubscriptionsFor<Core.Models.MvxReloaderMessage>())
                //{
                //    var message = new Core.Models.MvxReloaderMessage(this, this.Dyno);

                //    this._messenger.Publish(message);
                //}

                this.SaveButton = false;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Sets the active dyno.
        /// </summary>
        /// <param name="dyno">The dyno.</param>
        /// <returns>aktualisiertes DynoModel.</returns>
        protected DynoModel SetActiveDyno(DynoModel dyno)
        {
            try
            {
                dyno.Active = true;

                // auch in db updaten.
                _vehicleService.UpdateOne(dyno);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);

                // z.B. kein Dyno aktiv z.B. nicht in datenbank
            }

            return dyno;
        }

        /// <summary>
        /// Sets the inactive dyno.
        /// </summary>
        protected void SetInactiveDyno()
        {
            try
            {
                // aktiven Dyno inaktiv setzen.
                this.Dyno.Active = false;

                // wenn in db, dann auch da updaten.
                _vehicleService.UpdateOne(this.Dyno);

                // in lokaler liste aktualisieren.
                Dynos[_dynoIndex] = this.Dyno;
                // foreach (var dyno in Dynos) { dyno.Active = false; }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);

                // z.B. keine Werte in Dynos z.B. kein Dyno aktiv z.B. nicht in datenbank
            }
        }

        /// <summary>
        /// Imports the dyno.
        /// </summary>
        private async Task ImportDyno()
        {
            await Functions.GetPermission<Permissions.StorageRead>();
            await Functions.GetPermission<Permissions.StorageWrite>();

            await _browserService.DownloadDocumentAsync(
                "https://simtuning.tuke-productions.de/wp-content/uploads/DataExport.zip",
                SimTuning.Core.GeneralSettings.DataExportArchivePath);

            // zip extrahieren
            if (File.Exists(SimTuning.Core.GeneralSettings.DataExportFilePath))
            {
                File.Delete(SimTuning.Core.GeneralSettings.DataExportFilePath);
            }
            if (File.Exists(SimTuning.Core.GeneralSettings.AudioAccelerationFilePath))
            {
                File.Delete(SimTuning.Core.GeneralSettings.AudioAccelerationFilePath);
            }
            ZipFile.ExtractToDirectory(SimTuning.Core.GeneralSettings.DataExportArchivePath, Data.DatabaseSettings.FileDirectory);

            // wenn Datei ausgewählt using (FileStream sourceStream = File.Open(fileName, FileMode.OpenOrCreate)) { status =
            // SimTuning.Core.Helpers.AudioUtils.AudioCopy(SimTuning.Core.GeneralSettings.AudioFile, sourceStream); }

            // if (status) { await this.RefreshAudioFileAsync().ConfigureAwait(true); }

            // TODO: only for testing
            if (File.Exists(SimTuning.Core.GeneralSettings.DataExportFilePath))
            {
                string json = await File.ReadAllTextAsync(SimTuning.Core.GeneralSettings.DataExportFilePath);
                DynoModel dyno = JsonConvert.DeserializeObject<DynoModel>(json);
            }
        }

        #endregion Methods

        #region Values

        #region Commands

        /// <summary>
        /// Gets or sets the delete dyno command.
        /// </summary>
        /// <value>The delete dyno command.</value>
        public IRelayCommand DeleteDynoCommand { get; set; }

        /// <summary>
        /// Gets or sets the export dyno command.
        /// </summary>
        /// <value>The export dyno command.</value>
        public IRelayCommand ExportDynoCommand { get; set; }

        /// <summary>
        /// Gets or sets the import dyno command.
        /// </summary>
        /// <value>The import dyno command.</value>
        public IAsyncRelayCommand ImportDynoCommand { get; set; }

        /// <summary>
        /// Creates new dynocommand.
        /// </summary>
        /// <value>The new dyno command.</value>
        public IRelayCommand NewDynoCommand { get; set; }

        /// <summary>
        /// Gets or sets the save dyno command.
        /// </summary>
        /// <value>The save dyno command.</value>
        public IRelayCommand SaveDynoCommand { get; set; }

        /// <summary>
        /// Gets or sets the show save button command.
        /// </summary>
        /// <value>The show save button command.</value>
        public IRelayCommand ShowSaveButtonCommand { get; set; }

        #endregion Commands

        protected readonly INavigationService _navigationService;
        private readonly IBrowserService _browserService;
        private readonly ILogger<DataViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private bool _createNewVehicle;
        private DynoModel _dyno;
        private int _dynoIndex;
        private ObservableCollection<DynoModel> _dynos;
        private bool _saveButton;

        private bool _takeExistingVehicle;

        private VehiclesModel _vehicle;

        /// <summary>
        /// Gets or sets a value indicating whether [create new vehicle].
        /// </summary>
        /// <value><c>true</c> if [create new vehicle]; otherwise, <c>false</c>.</value>
        public bool CreateNewVehicle
        {
            get => this._createNewVehicle;
            set => this.SetProperty(ref this._createNewVehicle, value);
        }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public DynoModel Dyno
        {
            get => this._dyno;
            set
            {
                // alten ausgewälten Dyno-Datensatz inaktiv setzen
                if (Dyno != null)
                    this.SetInactiveDyno();

                // fahrzeug zurücksetzen
                this.Vehicle = null;

                if (value != null)
                {
                    _dynoIndex = Dynos.IndexOf(value);

                    // Active setzen
                    value = this.SetActiveDyno(value);

                    // Vehicle laden
                    if (value.VehicleId != 0)
                    {
                        this.CreateNewVehicle = false;
                        this.TakeExistingVehicle = true;
                        this.Vehicle = this.Vehicles.Single(v => v.Id == value.VehicleId);
                    }
                    else
                    {
                        this.CreateNewVehicle = true;
                        this.TakeExistingVehicle = false;
                    }
                }
                else
                {
                    this.CreateNewVehicle = false;
                    this.TakeExistingVehicle = false;
                }

                this.SetProperty(ref this._dyno, value);

                this.OnPropertyChanged(nameof(this.DynoBeschreibung));
                this.OnPropertyChanged(nameof(this.DynoName));

                // Da werte in UI geändert wurden, wird save-button angezeigt, daher nach dem laden wieder disablen
                this.SaveButton = false;
            }
        }

        /// <summary>
        /// Gets or sets the dyno beschreibung.
        /// </summary>
        /// <value>The dyno beschreibung.</value>
        public string DynoBeschreibung
        {
            get => this.Dyno?.Beschreibung;
            set
            {
                if (this.Dyno == null)
                {
                    return;
                }

                this.Dyno.Beschreibung = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the dyno.
        /// </summary>
        /// <value>The name of the dyno.</value>
        public string DynoName
        {
            get => this.Dyno?.Name;
            set
            {
                if (this.Dyno == null)
                {
                    return;
                }

                this.Dyno.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the dynos.
        /// </summary>
        /// <value>The dynos.</value>
        public ObservableCollection<DynoModel> Dynos
        {
            get => this._dynos;
            set => this.SetProperty(ref this._dynos, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [save button].
        /// </summary>
        /// <value><c>true</c> if [save button]; otherwise, <c>false</c>.</value>
        public bool SaveButton
        {
            get => this._saveButton;
            set => this.SetProperty(ref this._saveButton, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [take existing vehicle].
        /// </summary>
        /// <value><c>true</c> if [take existing vehicle]; otherwise, <c>false</c>.</value>
        public bool TakeExistingVehicle
        {
            get => this._takeExistingVehicle;
            set => this.SetProperty(ref this._takeExistingVehicle, value);
        }

        /// <summary>
        /// Gets or sets the vehicle.
        /// </summary>
        /// <value>The vehicle.</value>
        public VehiclesModel Vehicle
        {
            get => this._vehicle;
            set
            {
                SetProperty(ref this._vehicle, value);
                this.SaveButton = true;
            }
        }

        /// <summary>
        /// Gets or sets the vehicles.
        /// </summary>
        /// <value>The vehicles.</value>
        public ObservableCollection<VehiclesModel> Vehicles
        {
            get => new ObservableCollection<VehiclesModel>(_vehicleService.RetrieveVehicles());
        }

        #endregion Values
    }
}