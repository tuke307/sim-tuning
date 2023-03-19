// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimTuning.Core;
using SimTuning.Core.Helpers;
using SimTuning.Core.Models.Messages;
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

            this.DeleteDynoCommand = new RelayCommand(this.DeleteDyno);
            this.SaveDynoCommand = new RelayCommand(this.SaveDyno);

            this.ImportDynoCommand = new AsyncRelayCommand(this.ImportDyno);

            this.ExportDynoCommand = new AsyncRelayCommand(this.ExportDynoAsync);

            this.Dynos = new ObservableCollection<DynoModel>(_vehicleService.RetrieveDynos());
            Messenger.Register<DynoDataViewModel, CurrentDynoRequestMessage>(this, (r, m) => m.Reply(r.Dyno));
        }

        #region Methods

        /// <summary>
        /// Deletes the dyno.
        /// </summary>
        protected void DeleteDyno()
        {
            try
            {
                // in Datenbank löschen
                _vehicleService.DeleteOne(this.Dyno);

                // in lokaler liste löschen
                this.Dynos.Remove(this.Dyno);

                this.Dyno = null;
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
        public void NewDyno(VehiclesModel vehicle)
        {
            try
            {
                DynoModel dyno = new DynoModel()
                {
                    Name = "Dyno-Durchgang",
                    Beschreibung = $"Erstellt am {DateTime.Now} über Dyno-Modul",
                    VehicleId = (int)vehicle.Id,
                };
                dyno = _vehicleService.CreateOne(dyno);
                dyno.Vehicle = vehicle;

                this.Dynos.Add(dyno);
                this.Dyno = this.Dynos.Last();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
            }
        }

        /// <summary>
        /// Saves the dyno.
        /// </summary>
        protected void SaveDyno()
        {
            try
            {
                _vehicleService.UpdateOne(this.Dyno);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message, null);
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

        public IRelayCommand DeleteDynoCommand { get; set; }

        public IRelayCommand ExportDynoCommand { get; set; }

        public IAsyncRelayCommand ImportDynoCommand { get; set; }

        public IRelayCommand SaveDynoCommand { get; set; }

        #endregion Commands

        protected readonly INavigationService _navigationService;
        private readonly IBrowserService _browserService;
        private readonly ILogger<DynoDataViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private DynoModel _dyno;
        private ObservableCollection<DynoModel> _dynos;
        private VehiclesModel _vehicle;

        public DynoModel Dyno
        {
            get => this._dyno;
            set
            {
                if (value == null)
                {
                    // gerade gelöscht => letztes Vehicle neu laden
                    if (this.Dynos.Count != 0)
                    {
                        value = this.Dynos.Last();
                    }
                }

                this.SetProperty(ref this._dyno, value);

                raiseAllPropertyChanged();

                Messenger.Send(new DynoChangedMessage(Dyno));
            }
        }

        private void raiseAllPropertyChanged()
        {
            this.OnPropertyChanged(nameof(this.DynoBeschreibung));
            this.OnPropertyChanged(nameof(this.DynoName));
        }

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

        public ObservableCollection<DynoModel> Dynos
        {
            get => this._dynos;
            set => this.SetProperty(ref this._dynos, value);
        }

        public VehiclesModel Vehicle
        {
            get => this._vehicle;
            set => SetProperty(ref this._vehicle, value);
        }

        #endregion Values
    }
}