// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using SimTuning.Core.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimTuning.Core.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _services;

        public NavigationService(IServiceProvider services)
        => _services = services;

        protected INavigation Navigation
        {
            get
            {
                INavigation? navigation = Application.Current?.MainPage?.Navigation;
                if (navigation is not null)
                    return navigation;
                else
                {
                    //This is not good!
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    throw new Exception();
                }
            }
        }

        public Task Navigate<T>() where T : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateBack()
        {
            if (Navigation.NavigationStack.Count > 1)
                return Navigation.PopAsync();
            throw new InvalidOperationException("No pages to navigate back to!");
        }

        public Task NavigatePage<T>() where T : Page
        {
            var page = _services.GetService<T>();
            if (page is not null)
                return Navigation.PushAsync(page, true);
            throw new InvalidOperationException($"Unable to resolve type {typeof(T).FullName}");
        }

    }
}