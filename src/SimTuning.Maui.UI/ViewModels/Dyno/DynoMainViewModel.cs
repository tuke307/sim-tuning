// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using SimTuning.Core.Models.Messages;
using SimTuning.Maui.UI.Services;

namespace SimTuning.Maui.UI.ViewModels
{
    public class DynoMainViewModel : ViewModelBase
    {
        public DynoMainViewModel(
            ILogger<DynoMainViewModel> logger,
            INavigationService navigationService)
        {
            _logger = logger;
            _navigationService = navigationService;

            Messenger.Register<DynoMainViewModel, DynoChangedMessage>(this, (r, m) => r.PageTitle = m.Value.Name);
        }

        #region Methods

        protected override void OnActivated()
        {
        }

        #endregion Methods

        #region Values

        protected readonly INavigationService _navigationService;

        private readonly ILogger<DynoMainViewModel> _logger;
        private int _dynoTabIndex;
        private string? _pageTitle;

        public int DynoTabIndex
        {
            get => _dynoTabIndex;
            set => SetProperty(ref _dynoTabIndex, value);
        }

        public string? PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        #endregion Values
    }
}