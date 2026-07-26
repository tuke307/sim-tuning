// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Helpers;
using SimTuning.Core.Models.Messages;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimTuning.Maui.UI.ViewModels
{

    public partial class DynoDataViewModel : ViewModelBase
    {
        /// <summary>
        /// JSON serializer options for dyno export/import. <see cref="ReferenceHandler.Preserve" />
        /// handles the Dyno↔Vehicle reference loop the same way Newtonsoft's
        /// <c>PreserveReferencesHandling.Objects</c> did (Phase 14 A08 migration).
        /// </summary>
        private static readonly JsonSerializerOptions ExportJsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
        };

        private readonly IPopupService _popupService;

        public DynoDataViewModel(
            ILogger<DynoDataViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService,
            IBrowserService browserService,
            IPopupService popupService)
        {
            _logger = logger;
            _navigationService = navigationService;
            _vehicleService = vehicleService;
            _popupService = popupService;
            _browserService = browserService;

            // Commands are source-generated via [RelayCommand] on the methods below.

            Dynos = new ObservableCollection<DynoModel>(_vehicleService.RetrieveDynos() ?? new List<DynoModel>());
            Messenger.Register<DynoDataViewModel, CurrentDynoRequestMessage>(this, (r, m) => m.Reply(r.Dyno!)); // justified: preserves prior behavior — reply carries the current dyno (incl. null when none selected)
        }

        #region Methods

        /// <summary>
        /// Deletes the dyno.
        /// </summary>
        [RelayCommand]
        protected void DeleteDyno()
        {
            try
            {
                // in Datenbank löschen
                _vehicleService.DeleteOne(Dyno!); // justified: preserves prior behavior — Dyno could already be null; service call is wrapped in try/catch

                // in lokaler liste löschen
                Dynos!.Remove(Dyno!); // justified: Dynos reliably initialized in ctor; Dyno preserved-as-before (see above)

                Dyno = null;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error deleting dyno: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Exports the dyno.
        /// </summary>
        [RelayCommand]
        protected async Task ExportDynoAsync()
        {
            try
            {
                // erstellen der json. (Phase 14 A08: migrated from Newtonsoft.Json to System.Text.Json.)
                string json = JsonSerializer.Serialize(Dyno, ExportJsonOptions);

                await File.WriteAllTextAsync(GeneralSettings.DataExportFilePath, json);

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
                _logger.LogError(exc, "Error exporting dyno: {Message}", exc.Message);
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
                    VehicleId = vehicle.Id.GetValueOrDefault(),
                };
                dyno = _vehicleService.CreateOne(dyno)!; // justified: on DB failure CreateOne returns null -> NRE here lands in the surrounding catch (preserves prior error logging)
                dyno.Vehicle = vehicle;

                Dynos!.Add(dyno); // justified: Dynos reliably initialized in ctor
                Dyno = Dynos!.Last(); // justified: Dynos reliably initialized in ctor
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error creating new dyno: {Message}", exc.Message);
            }
        }

        [RelayCommand]
        private async Task ShowNewDynoAsync()
        {
            var result = await _popupService.ShowPopupAsync<VehiclesViewModel>(Shell.Current);
            if (result is VehiclesModel vehicleModel)
            {
                NewDyno(vehicleModel);
            }
        }

        /// <summary>
        /// Saves the dyno.
        /// </summary>
        [RelayCommand]
        protected void SaveDyno()
        {
            try
            {
                _vehicleService.UpdateOne(Dyno!); // justified: preserves prior behavior — Dyno could already be null; service call is wrapped in try/catch
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error saving dyno: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Imports the dyno.
        /// </summary>
        [RelayCommand]
        private async Task ImportDyno()
        {
            await Functions.GetPermission<Permissions.StorageRead>();
            await Functions.GetPermission<Permissions.StorageWrite>();


            await _browserService.DownloadDocumentAsync(
                "https://simtuning.tony-luke.de/wp-content/uploads/DataExport.zip",
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

            // Zip-Slip defense (Phase 14 A01): ExtractToDirectory(Async) performs no per-entry
            // validation, so a crafted archive could write outside the target directory. Open the
            // archive and reject any entry whose canonicalized path escapes FileDirectory. The sync
            // OpenRead/ExtractToFile calls are fast local-file IO; AsyncFixer02 is suppressed for
            // this block only (per-entry validation can't be done via ExtractToDirectoryAsync).
            string destinationDirectory = Data.DatabaseSettings.FileDirectory;
            string fullDestinationDirectory =
                Path.GetFullPath(destinationDirectory) + Path.DirectorySeparatorChar;

#pragma warning disable AsyncFixer02
            using (ZipArchive archive = ZipFile.OpenRead(SimTuning.Core.GeneralSettings.DataExportArchivePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fullTarget = Path.GetFullPath(Path.Combine(fullDestinationDirectory, entry.FullName));
                    if (!fullTarget.StartsWith(fullDestinationDirectory, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"Refusing to extract zip entry '{entry.FullName}' — it escapes the target directory.");
                    }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        // Directory entry — create and skip.
                        Directory.CreateDirectory(fullTarget);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(fullTarget)!);
                    entry.ExtractToFile(fullTarget, overwrite: true);
                }
            }
#pragma warning restore AsyncFixer02

            /*

            using var stream = await FileSystem.OpenAppPackageFileAsync(GeneralSettings.AudioAccelerationFile);
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();
            */

            // wenn Datei ausgewählt using (FileStream sourceStream = File.Open(fileName, FileMode.OpenOrCreate)) { status =
            // SimTuning.Core.Helpers.AudioUtils.AudioCopy(SimTuning.Core.GeneralSettings.AudioFile, sourceStream); }

            // if (status) { await RefreshAudioFileAsync().ConfigureAwait(true); }

            // TODO: only for testing
            /*
            if (File.Exists(SimTuning.Core.GeneralSettings.DataExportFilePath))
            {
                string json = await File.ReadAllTextAsync(SimTuning.Core.GeneralSettings.DataExportFilePath);
                DynoModel dyno = JsonSerializer.Deserialize<DynoModel>(json, ExportJsonOptions)!;
            }
            */
        }

        #endregion Methods

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly IBrowserService _browserService;
        private readonly ILogger<DynoDataViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private DynoModel? _dyno;
        private ObservableCollection<DynoModel>? _dynos;
        private VehiclesModel? _vehicle;

        public DynoModel? Dyno
        {
            get => _dyno;
            set
            {
                if (value == null)
                {
                    // Just deleted => load last dyno
                    var dynos = Dynos;
                    if (dynos != null && dynos.Count > 0)
                    {
                        value = dynos.Last();
                    }
                }

                SetProperty(ref _dyno, value);

                raiseAllPropertyChanged();

                Messenger.Send(new DynoChangedMessage(Dyno!)); // justified: preserves prior behavior — subscribers historically receive the current dyno (incl. null when none selected)
            }
        }

        private void raiseAllPropertyChanged()
        {
            OnPropertyChanged(nameof(DynoBeschreibung));
            OnPropertyChanged(nameof(DynoName));
        }

        public string? DynoBeschreibung
        {
            get => Dyno?.Beschreibung;
            set
            {
                if (Dyno == null)
                {
                    return;
                }

                Dyno.Beschreibung = value;
            }
        }

        public string? DynoName
        {
            get => Dyno?.Name;
            set
            {
                if (Dyno == null)
                {
                    return;
                }

                Dyno.Name = value;
            }
        }

        public ObservableCollection<DynoModel>? Dynos
        {
            get => _dynos;
            set => SetProperty(ref _dynos, value);
        }

        public VehiclesModel? Vehicle
        {
            get => _vehicle;
            set => SetProperty(ref _vehicle, value);
        }

        #endregion Values
    }
}
