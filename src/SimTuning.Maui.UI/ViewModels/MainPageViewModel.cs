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
        }

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<MainPageViewModel> _logger;

        #endregion Values
    }
}