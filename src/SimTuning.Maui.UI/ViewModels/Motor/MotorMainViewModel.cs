// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.ViewModels
{
    using Microsoft.Extensions.Logging;
    using CommunityToolkit.Mvvm.ComponentModel;
    using System.Threading.Tasks;
    using SimTuning.Core.Services;using SimTuning.Maui.UI.Services;

    public class MotorMainViewModel : ViewModelBase
    {
        public MotorMainViewModel(
            ILogger<MotorMainViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService ;
        }

        #region Methods


        #endregion Methods

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<MotorMainViewModel> _logger;
        private int _motorTabIndex;

        public int MotorTabIndex
        {
            get => _motorTabIndex;
            set { SetProperty(ref _motorTabIndex, value); }
        }

        #endregion Values
    }
}