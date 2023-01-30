// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using SimTuning.Maui.UI.Services;

namespace SimTuning.Maui.UI.ViewModels.Einstellungen
{
    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel(
            ILogger<MenuViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
        }

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<MenuViewModel> _logger;

        #endregion Values
    }
}