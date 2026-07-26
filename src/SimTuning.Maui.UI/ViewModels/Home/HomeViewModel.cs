// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using System.Threading.Tasks;

namespace SimTuning.Maui.UI.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        public HomeViewModel(
            ILogger<HomeViewModel> logger,
            INavigationService navigationService,
            IBrowserService browserService)
        {
            _logger = logger;
            _browserService = browserService;

            // Commands are source-generated via [RelayCommand] on the methods below.
        }

        #region Methods

        /// <summary>Opens the Instagram page in the browser.</summary>
        [RelayCommand]
        private void OpenInstagram() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyInstagram);

        /// <summary>Opens the website in the browser.</summary>
        [RelayCommand]
        private void OpenWebsite() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyWebsite);

        /// <summary>Opens the Twitter page in the browser.</summary>
        [RelayCommand]
        private void OpenTwitter() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MyTwitter);

        /// <summary>Opens the email link in the browser.</summary>
        [RelayCommand]
        private void OpenEmail() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.MailLink);

        /// <summary>Opens the donation page in the browser.</summary>
        [RelayCommand]
        private void OpenDonate() => _browserService.OpenBrowser(SimTuning.Core.WebsiteConstants.Paypaldonation);

        #endregion Methods



        #region Values

        private readonly IBrowserService _browserService;
        private readonly ILogger<HomeViewModel> _logger;

        #endregion Values
    }
}