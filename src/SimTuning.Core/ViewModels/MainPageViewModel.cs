// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Core.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Extensions.Logging;
    using SimTuning.Core.Services;
    using SimTuning.Core.ViewModels.Home;
    using System.Threading.Tasks;

    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(
            ILogger<MainPageViewModel> logger,
            INavigationService INavigationService)
        {
            this._logger = logger;
            this._INavigationService = INavigationService;

            this.ShowHomeViewModelCommand = new AsyncRelayCommand(() => _INavigationService.Navigate<HomeViewModel>());
            this.ShowMenuViewModelCommand = new AsyncRelayCommand(() => _INavigationService.Navigate<MenuViewModel>());
        }

        #region Methods

        public void ViewAppeared()
        {
            //this.ShowMenuViewModelCommand.Execute();
            //this.ShowHomeViewModelCommand.Execute();
        }

        #endregion Methods

        #region Values

        protected readonly INavigationService _INavigationService;
        private readonly ILogger<MainPageViewModel> _logger;

        /// <summary>
        /// Gets or sets the show home view model command.
        /// </summary>
        /// <value>The show home view model command.</value>
        public IAsyncRelayCommand ShowHomeViewModelCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the show menu view model command.
        /// </summary>
        /// <value>The show menu view model command.</value>
        public IAsyncRelayCommand ShowMenuViewModelCommand { get; protected set; }

        #endregion Values
    }
}