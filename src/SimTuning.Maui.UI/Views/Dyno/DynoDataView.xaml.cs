// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Core.Helpers;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoDataView : ContentView
    {
        public DynoDataViewModel ViewModel => (DynoDataViewModel)BindingContext;

        public DynoDataView()
        {
            this.InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DynoDataViewModel>();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
#if __MOBILE__
            Navigation.PushModalAsync(new DynoRuntimeView());
#else
            Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_ONLYMOBILE"));
#endif
        }
    }
}