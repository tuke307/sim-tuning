// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data;
using SimTuning.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;

namespace SimTuning.Core.ViewModels.Dyno
{
    public class GeschwindigkeitViewModel : ViewModelBase
    {
        public GeschwindigkeitViewModel(
            ILogger<GeschwindigkeitViewModel> logger,
            INavigationService INavigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._INavigationService = INavigationService;
            this._vehicleService = vehicleService;

            this.ShowAusrollenCommand = new AsyncRelayCommand(async () => await _INavigationService.Navigate<Dyno.AusrollenViewModel>());

            this.RefreshPlotCommand = new AsyncRelayCommand(this.RefreshPlot);

            this.ReloadData();
        }

        #region Values

        protected readonly INavigationService _INavigationService;
        protected readonly IVehicleService _vehicleService;
        private readonly ILogger<GeschwindigkeitViewModel> _logger;
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

        public ISeries PlotGeschwindigkeit
        {
            get => DynoLogic.PlotGeschwindigkeit;
        }

        public IAsyncRelayCommand RefreshPlotCommand { get; set; }

        public IAsyncRelayCommand ShowAusrollenCommand { get; set; }

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
        /// Überprüft ob wichtige Dyno-Audio-Daten vorhanden sind.
        /// </summary>
        private bool CheckDynoData()
        {
            if (this.Dyno == null)
            {
                Helpers.Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            if (this.Dyno.Geschwindigkeit == null)
            {
                Helpers.Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Aktualisiert den Beschleunigungs-Graphen.
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

                DynoLogic.GetGeschwindigkeitsGraphFitted(this.Dyno?.Geschwindigkeit.ToList());

                this.OnPropertyChanged(nameof(PlotGeschwindigkeit));

                //await loadingDialog.DismissAsync().ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger.LogError("Fehler bei RefreshPlot: ", exc);
            }
        }

        #endregion Methods
    }
}