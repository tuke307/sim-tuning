// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ViewModels.Einstellungen
{
    using Microsoft.Extensions.Logging;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using System.Threading.Tasks;
    using SimTuning.Core.Services;

    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel(
            ILogger<MenuViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService ;

            this.OpenVehiclesCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Einstellungen.VehiclesViewModel>());
            this.OpenApplicationCommand = new AsyncRelayCommand(() => this._navigationService.Navigate<Einstellungen.ApplicationViewModel>());
        }

        #region Methods

       

        #endregion Methods

        #region Values

        protected readonly INavigationService _navigationService;
        private readonly ILogger<MenuViewModel> _logger;

        public IAsyncRelayCommand OpenApplicationCommand { get; set; }
        public IAsyncRelayCommand OpenVehiclesCommand { get; set; }

        #endregion Values
    }
}