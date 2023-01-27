// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using SimTuning.Core.Services;

namespace SimTuning.Core.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(
            ILogger<MainPageViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService ;
        }

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<MainPageViewModel> _logger;

        #endregion Values
    }
}