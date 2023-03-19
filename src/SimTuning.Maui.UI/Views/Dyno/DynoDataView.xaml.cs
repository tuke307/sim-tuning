// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Core.Helpers;
using SimTuning.Data.Models;
using SimTuning.Maui.UI.ViewModels;
using SimTuning.Maui.UI.Views.Popups;

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

        private async void NewDynoButton_Clicked(object sender, EventArgs e)
        {
            var task = Application.Current.MainPage.ShowPopupAsync(new DynoCreationPopup());
            var result = await task;
            if (result != null)
            {
                ViewModel.NewDyno(result as VehiclesModel);
            }
        }
    }
}