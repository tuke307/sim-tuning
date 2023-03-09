// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;

namespace SimTuning.Maui.UI.ViewModels
{
    public class EinlassMainViewModel : ViewModelBase
    {
        public EinlassMainViewModel(
            ILogger<EinlassMainViewModel> logger)
        {
            this._logger = logger;
        }

        #region Methods

        #endregion Methods

        #region Values

        private readonly ILogger<EinlassMainViewModel> _logger;
        private int _einlassTabIndex;

        public int EinlassTabIndex
        {
            get => _einlassTabIndex;
            set => SetProperty(ref _einlassTabIndex, value);
        }

        #endregion Values
    }
}