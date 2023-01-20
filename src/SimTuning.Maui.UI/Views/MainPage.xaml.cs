// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Core.ViewModels;

namespace SimTuning.Maui.UI.Views;

public partial class MainPage : FlyoutPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = Ioc.Default.GetRequiredService<MainPageViewModel>();
    }

    public MainPageViewModel ViewModel => (MainPageViewModel)BindingContext;
}