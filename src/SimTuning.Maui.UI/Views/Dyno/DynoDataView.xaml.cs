// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Maui.UI.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;


namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoDataView : ContentPage
    {
        public DynoDataView()
        {
            this.InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DynoDataViewModel>();
        }

        public DynoDataViewModel ViewModel => (DynoDataViewModel)BindingContext;
    }
}