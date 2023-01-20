// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ViewModels.Motor
{
    using Microsoft.Extensions.Logging;
    using CommunityToolkit.Mvvm.ComponentModel;
    using System.Threading.Tasks;
    using SimTuning.Core.Services;

    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService INavigationService)
        {
            this._logger = logger;
            this._INavigationService = INavigationService;
        }

        #region Methods


        #endregion Methods

        #region Values

        protected readonly INavigationService _INavigationService;
        private readonly ILogger<MainViewModel> _logger;
        private int _motorTabIndex;

        public int MotorTabIndex
        {
            get => _motorTabIndex;
            set { SetProperty(ref _motorTabIndex, value); }
        }

        #endregion Values
    }
}