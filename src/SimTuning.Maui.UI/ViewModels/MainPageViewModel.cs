// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimTuning.Core.Helpers;
using SimTuning.Core.Services;
using SimTuning.Data;
using SimTuning.Maui.UI.Services;

namespace SimTuning.Maui.UI.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(
            ILogger<MainPageViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService;

            InitializeDatabaseAsync();
        }

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

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<MainPageViewModel> _logger;

        #endregion Values
    }
}