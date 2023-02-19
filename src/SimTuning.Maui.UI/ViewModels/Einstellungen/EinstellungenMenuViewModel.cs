// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using SimTuning.Maui.UI.Services;

namespace SimTuning.Maui.UI.ViewModels
{
    public class EinstellungenMenuViewModel : ViewModelBase
    {
        public EinstellungenMenuViewModel(
            ILogger<EinstellungenMenuViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
        }

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<EinstellungenMenuViewModel> _logger;

        #endregion Values
    }
}