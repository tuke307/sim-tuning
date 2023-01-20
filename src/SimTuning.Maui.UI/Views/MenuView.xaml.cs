// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Core.ViewModels;

namespace SimTuning.Maui.UI.Views;

public partial class MenuView : ContentPage
{
    public MenuView()
    {
        InitializeComponent();

        BindingContext = Ioc.Default.GetRequiredService<MenuViewModel>();
    }

    public MenuViewModel ViewModel => (MenuViewModel)BindingContext;
}