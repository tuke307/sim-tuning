// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using System.Threading.Tasks;

namespace SimTuning.Maui.UI.ViewModels
{
    public class EinstellungenApplicationViewModel : ViewModelBase
    {
        /// <summary> Initializes a new instance of the <see cref="EinstellungenApplicationViewModel"/> class. </summary> <param name="logger"><inheritdoc cref="ILogger" path="/summary/node()"
        /// /></param> <param name="INavigationService"><inheritdoc cref="INavigationService" path="/summary/node()" /></param
        public EinstellungenApplicationViewModel(
            ILogger<EinstellungenApplicationViewModel> logger,
            INavigationService navigationService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
        }

        #region Methods

        private readonly ILogger<EinstellungenApplicationViewModel> _logger;

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