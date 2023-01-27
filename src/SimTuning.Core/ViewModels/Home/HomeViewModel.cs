// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SimTuning.Core.Services;
using System.Threading.Tasks;

namespace SimTuning.Core.ViewModels.Home
{
    public class HomeViewModel : ViewModelBase
    {
        public HomeViewModel(
            ILogger<HomeViewModel> logger,
            INavigationService navigationService,
            IBrowserService browserService)
        {
            this._logger = logger;
            this._browserService = browserService;

            this.OpenInstagramCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyInstagram));
            this.OpenWebsiteCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyWebsite));
            this.OpenTwitterCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyTwitter));
            this.OpenEmailCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MailLink));
            this.OpenDonateCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.Paypaldonation));
            this.OpenTutorialCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.TutorialWebsite));
        }

        #region Commands


        #endregion Commands

        #region Values

        private readonly IBrowserService _browserService;
        private readonly ILogger<HomeViewModel> _logger;

        public IRelayCommand OpenDonateCommand { get; set; }

        public IRelayCommand OpenEmailCommand { get; set; }

        public IRelayCommand OpenInstagramCommand { get; set; }

        public IRelayCommand OpenTutorialCommand { get; set; }

        public IRelayCommand OpenTwitterCommand { get; set; }

        public IRelayCommand OpenWebsiteCommand { get; set; }

        #endregion Values
    }
}