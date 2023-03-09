// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;

namespace SimTuning.Maui.UI.ViewModels
{
    public class AuslassMainViewModel : ViewModelBase
    {
        public AuslassMainViewModel(
            ILogger<AuslassMainViewModel> logger)
        {
            this._logger = logger;
        }


        #region Values

        private readonly ILogger<AuslassMainViewModel> _logger;
        private int _auslassTabIndex;

        public int AuslassTabIndex
        {
            get => _auslassTabIndex;
            set { SetProperty(ref _auslassTabIndex, value); }
        }

        #endregion Values
    }
}