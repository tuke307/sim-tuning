// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using SimTuning.Maui.UI.Services;

namespace SimTuning.Maui.UI.ViewModels
{
    public class DynoMainViewModel : ViewModelBase
    {
        public DynoMainViewModel(
            ILogger<DynoMainViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
        }

        #region Methods

        #endregion Methods

        #region Values

        protected readonly INavigationService _navigationService;

        private readonly ILogger<DynoMainViewModel> _logger;
        private int _dynoTabIndex;

        public int DynoTabIndex
        {
            get => _dynoTabIndex;
            set => SetProperty(ref _dynoTabIndex, value);
        }

        #endregion Values
    }
}