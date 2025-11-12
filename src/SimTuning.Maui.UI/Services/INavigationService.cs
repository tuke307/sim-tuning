// Copyright (c) 2025 tuke productions. All rights reserved.

namespace SimTuning.Maui.UI.Services
{
    public interface INavigationService
    {
        Task Navigate<T>(object? parameter) where T : Page;
        Task NavigateBack();
    }
}