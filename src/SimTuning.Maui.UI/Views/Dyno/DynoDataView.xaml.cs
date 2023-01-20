// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.ViewModels.Dyno;
using CommunityToolkit.Mvvm.DependencyInjection;


namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoDataView : ContentPage
    {
        public DynoDataView()
        {
            this.InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DataViewModel>();
        }

        public DataViewModel ViewModel => (DataViewModel)BindingContext;
    }
}