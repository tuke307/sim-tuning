// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using Microsoft.Extensions.Logging;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimTuning.Maui.UI.ViewModels.Dyno
{
    public class AusrollenViewModel : ViewModelBase
    {
        public AusrollenViewModel(
            ILogger<AusrollenViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._navigationService = navigationService;
            this._vehicleService = vehicleService;

            //this.ShowDiagnosisCommand = new AsyncRelayCommand(async () => await _navigationService.Navigate<SimTuning.Maui.UI.Views.Dyno.DynoDiagnosisView>(null));

            this.RefreshPlotCommand = new AsyncRelayCommand(this.RefreshPlot);
            this.ReloadData();
        }

        #region Values

        protected readonly INavigationService _navigationService;
        protected readonly IVehicleService _vehicleService;
        private readonly ILogger<AusrollenViewModel> _logger;
        private DynoModel _dyno;

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public DynoModel Dyno
        {
            get => _dyno;
            set => SetProperty(ref _dyno, value);
        }

        /// <summary>
        /// PlotAusrollen.
        /// </summary>
        public ISeries PlotAusrollen
        {
            get => null;//DynoLogic.PlotAusrollen;
        }

        public IAsyncRelayCommand RefreshPlotCommand { get; set; }

        public IAsyncRelayCommand ShowDiagnosisCommand { get; set; }

        #endregion Values

        #region Methods

        /// <summary>
        /// Reloads the data.
        /// </summary>
        public void ReloadData()
        {
            try
            {
                this.Dyno = _vehicleService.RetrieveOneActive();
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei ReloadData: ", exc);
            }
        }

        /// <summary>
        /// Aktualisiert den Ausroll-Graphen.
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshPlot()
        {
            if (!this.CheckDynoData())
            {
                return;
            }

            try
            {
                //var loadingDialog = await DisplayAlert(message: SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "MES_LOAD")).ConfigureAwait(false);

                //DynoLogic.GetAusrollGraphFitted(this.Dyno?.Ausrollen.ToList());

                this.OnPropertyChanged(nameof(PlotAusrollen));

                //await loadingDialog.DismissAsync().ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei RefreshPlot: ", exc);
            }
        }

        /// <summary>
        /// Überprüft ob wichtige Dyno-Audio-Daten vorhanden sind.
        /// </summary>
        private bool CheckDynoData()
        {
            if (this.Dyno == null)
            {
                Core.Helpers.Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            if (this.Dyno.Ausrollen == null)
            {
                Core.Helpers.Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            return true;
        }

        #endregion Methods
    }
}