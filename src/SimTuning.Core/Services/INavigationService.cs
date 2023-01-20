// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using SimTuning.Core.ViewModels;
using System.Threading.Tasks;

namespace SimTuning.Core.Services
{
    public interface INavigationService
    {
        Task Navigate<T>() where T : ViewModelBase;
        Task NavigatePage<T>() where T : Page;
        Task NavigateBack();
    }
}