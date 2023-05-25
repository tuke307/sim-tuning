// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using SimTuning.Maui.UI.Services;

namespace SimTuning.Maui.UI.ViewModels
{
    public class EinstellungenViewModel : ViewModelBase
    {
        public EinstellungenViewModel(
            ILogger<EinstellungenViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
        }

        #region Methods

        private readonly ILogger<EinstellungenViewModel> _logger;

        #endregion Methods

        #region Values

        protected readonly INavigationService _navigationService;

        /// <summary>
        /// Gets or sets the rounding accuracy.
        /// </summary>
        /// <value>The rounding accuracy.</value>
        public int RoundingAccuracy
        {
            get => Data.UnitSettings.RoundingAccuracy;
            set
            {
                Data.UnitSettings.RoundingAccuracy = value;
                this.OnPropertyChanged(nameof(this.RoundingAccuracy));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [round on unit change].
        /// </summary>
        /// <value><c>true</c> if [round on unit change]; otherwise, <c>false</c>.</value>
        public bool RoundOnUnitChange
        {
            get => Data.UnitSettings.RoundOnUnitChange;
            set
            {
                Data.UnitSettings.RoundOnUnitChange = value;
                this.OnPropertyChanged(nameof(this.RoundOnUnitChange));
            }
        }

        #endregion Values
    }
}