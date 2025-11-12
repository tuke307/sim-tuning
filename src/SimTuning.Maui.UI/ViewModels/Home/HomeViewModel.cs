// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using System.Threading.Tasks;

namespace SimTuning.Maui.UI.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public HomeViewModel(
            ILogger<HomeViewModel> logger,
            INavigationService navigationService,
            IBrowserService browserService)
        {
            _logger = logger;
            _browserService = browserService;

            OpenInstagramCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyInstagram));
            OpenWebsiteCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyWebsite));
            OpenTwitterCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyTwitter));
            OpenEmailCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MailLink));
            OpenDonateCommand = new RelayCommand(() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.Paypaldonation));
        }



        #region Values

        private readonly IBrowserService _browserService;
        private readonly ILogger<HomeViewModel> _logger;

        public IRelayCommand OpenDonateCommand { get; set; }

        public IRelayCommand OpenEmailCommand { get; set; }

        public IRelayCommand OpenInstagramCommand { get; set; }

        public IRelayCommand OpenTwitterCommand { get; set; }

        public IRelayCommand OpenWebsiteCommand { get; set; }

        #endregion Values
    }
}