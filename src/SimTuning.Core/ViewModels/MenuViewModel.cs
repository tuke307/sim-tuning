// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ViewModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using SimTuning.Core.Models;
    using SimTuning.Data;
    using System.Threading.Tasks;
    using Microsoft.Maui.ApplicationModel;
    using System.IO;
    using SimTuning.Core.Helpers;
    using SimTuning.Core.Services;

    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel(
            ILogger<MenuViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService;

            this.ShowHomeCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Home.HomeViewModel>());
            this.ShowEinlassCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Einlass.MainViewModel>());
            this.ShowAuslassCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Auslass.MainViewModel>());
            this.ShowMotorCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Motor.MainViewModel>());
            this.ShowDynoCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Dyno.DataViewModel>());
            this.ShowEinstellungenCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Einstellungen.MenuViewModel>());
            this.InitializeDatabase = new AsyncRelayCommand(this.InitializeDatabaseAsync);

            //this.InitializeDatabase.ExecuteAsync();
        }

        #region Methods

        /// <summary>
        /// Initializes the database asynchronous.
        /// </summary>
        protected async Task InitializeDatabaseAsync()
        {
            // da die GetPermission Funktionen teilweise zu lange dauern oder die funktion beenden wird der Task davor gestartet.
            _ = Task.Run(async () =>
            {
                // 5 sekunden warten => solange ist zeit um die permission zu geben.
                await Task.Delay(5000).ConfigureAwait(true);

                // since android 10, database has to be created at the first time
                if (!File.Exists(Data.DatabaseSettings.DatabasePath))
                {
                    var fs = File.Create(Data.DatabaseSettings.DatabasePath);
                    fs.Dispose();
                }

                using (var db = new DatabaseContext())
                {
                    await db.Database.MigrateAsync().ConfigureAwait(true);
                    await db.Database.EnsureCreatedAsync().ConfigureAwait(true);
                }
            }
               ).ConfigureAwait(false);

            await Functions.GetPermission<Permissions.StorageRead>().ConfigureAwait(true);
            await Functions.GetPermission<Permissions.StorageWrite>().ConfigureAwait(true);
        }


        #endregion Methods

        #region Values

        #region Commands

        private readonly ILogger<MenuViewModel> _logger;

        public IRelayCommand ButtonCloseMenu { get; set; }

        public IRelayCommand ButtonOpenMenu { get; set; }

        public IAsyncRelayCommand InitializeDatabase { get; protected set; }

        public IAsyncRelayCommand ShowAuslassCommand { get; set; }

        public IAsyncRelayCommand ShowDynoCommand { get; set; }

        public IAsyncRelayCommand ShowEinlassCommand { get; set; }

        public IAsyncRelayCommand ShowEinstellungenCommand { get; set; }

        public IAsyncRelayCommand ShowHomeCommand { get; set; }

        public IAsyncRelayCommand ShowMotorCommand { get; set; }

        #endregion Commands

        protected readonly INavigationService _navigationService;

        #endregion Values
    }
}