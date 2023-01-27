// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using SimTuning.Core.Services;

namespace SimTuning.Core.ViewModels.Auslass
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService ;
        }


        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<MainViewModel> _logger;
        private int _auslassTabIndex;

        public int AuslassTabIndex
        {
            get => _auslassTabIndex;
            set { SetProperty(ref _auslassTabIndex, value); }
        }

        #endregion Values
    }
}