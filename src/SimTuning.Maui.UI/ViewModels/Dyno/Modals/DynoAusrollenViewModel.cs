// Copyright (c) 2025 tuke productions. All rights reserved.
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

namespace SimTuning.Maui.UI.ViewModels
{
    public class DynoAusrollenViewModel : ViewModelBase
    {
        public DynoAusrollenViewModel(
            ILogger<DynoAusrollenViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            _logger = logger;
            _navigationService = navigationService;
            _vehicleService = vehicleService;

            //ShowDiagnosisCommand = new AsyncRelayCommand(async () => await _navigationService.Navigate<SimTuning.Maui.UI.Views.Dyno.DynoDiagnosisView>(null));

            RefreshPlotCommand = new AsyncRelayCommand(RefreshPlot);
            ReloadData();
        }

        #region Values

        protected readonly INavigationService _navigationService;
        protected readonly IVehicleService _vehicleService;
        private readonly ILogger<DynoAusrollenViewModel> _logger;
        private DynoModel? _dyno;

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public DynoModel? Dyno
        {
            get => _dyno;
            set => SetProperty(ref _dyno, value);
        }

        /// <summary>
        /// PlotAusrollen.
        /// </summary>
        public ISeries? PlotAusrollen
        {
            get => null;//DynoLogic.PlotAusrollen;
        }

        public IAsyncRelayCommand RefreshPlotCommand { get; set; }

        public IAsyncRelayCommand? ShowDiagnosisCommand { get; set; }

        #endregion Values

        #region Methods

        /// <summary>
        /// Reloads the data.
        /// </summary>
        public void ReloadData()
        {
            try
            {
                Dyno = _vehicleService.RetrieveOneActive();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Fehler bei ReloadData: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Aktualisiert den Ausroll-Graphen.
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshPlot()
        {
            if (!CheckDynoData())
            {
                return;
            }

            try
            {
                //var loadingDialog = await DisplayAlert(message: SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "MES_LOAD")).ConfigureAwait(false);

                //DynoLogic.GetAusrollGraphFitted(Dyno?.Ausrollen.ToList());

                OnPropertyChanged(nameof(PlotAusrollen));

                //await loadingDialog.DismissAsync().ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Fehler beim Laden der Plots: {Message}", exc.Message);
            }
        }

        /// <summary>
        /// Überprüft ob wichtige Dyno-Audio-Daten vorhanden sind.
        /// </summary>
        private bool CheckDynoData()
        {
            if (Dyno == null)
            {
                Core.Helpers.Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            if (Dyno.Ausrollen == null)
            {
                Core.Helpers.Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_NODATA"));

                return false;
            }

            return true;
        }

        #endregion Methods
    }
}
