// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using Moq;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using Xunit;

namespace SimTuning.Test
{
    public class MauiViewModelsTest //: IViewModelTest
    {
        private readonly Mock<INavigationService> navigationServiceMock =
            new Mock<INavigationService>();

        private readonly Mock<IVehicleService> vehicleServiceMock =
            new Mock<IVehicleService>();

        /// <summary>
        /// AuslassAnwendungViewModelTest.
        /// </summary>
        [Fact]
        public void AuslassAnwendungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Core.ViewModels.Auslass.AnwendungViewModel>>();
            var vm = new Core.ViewModels.Auslass.AnwendungViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

            // Act
            vm.CalculateCommand.Execute(null);
            vm.DiffusorStageCommand.Execute(null);
            vm.InsertDataCommand.Execute(null);
        }
        /*
        /// <summary>
        /// AuslassMainViewModelTest.
        /// </summary>
        [Fact]
        public void AuslassMainViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Auslass.MainViewModel>();
        }

        /// <summary>
        /// Auslasses the theorie view model test.
        /// </summary>
        [Fact]
        public void AuslassTheorieViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Auslass.TheorieViewModel>();
            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the audio player view model test.
        /// </summary>
        [Fact]
        public void DynoAudioPlayerViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Dyno.AudioPlayerViewModel>();
            vm.CutBeginnCommand.Execute(null);
            vm.CutEndCommand.Execute(null);
            vm.DragCompletedCommand.Execute(null);
            vm.DragStartedCommand.Execute(null);
            vm.PlayPauseCommand.Execute(null);
            vm.SkipForwardCommand.Execute(null);
            vm.SkipBackwardsCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the ausrollen view model test.
        /// </summary>
        [Fact]
        public void DynoAusrollenViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Dyno.AusrollenViewModel>();
            vm.RefreshPlotCommand.Execute(null);
            vm.ShowDiagnosisCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the beschleunigung view model test.
        /// </summary>
        [Fact]
        public void DynoBeschleunigungViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Dyno.GeschwindigkeitViewModel>();
            vm.RefreshPlotCommand.Execute(null);
            vm.ShowAusrollenCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the data view model test.
        /// </summary>
        [Fact]
        public void DynoDataViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Dyno.DataViewModel>();
            vm.DeleteDynoCommand.Execute(null);
            vm.ExportDynoCommand.Execute(null);
            vm.NewDynoCommand.Execute(null);
            vm.SaveDynoCommand.Execute(null);
            vm.ShowSaveButtonCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the diagnosis view model test.
        /// </summary>
        [Fact]
        public void DynoDiagnosisViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Dyno.DiagnosisViewModel>();
            vm.InsertVehicleCommand.Execute(null);
            vm.RefreshPlotCommand.Execute(null);
            vm.ShowSaveCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the main view model test.
        /// </summary>
        [Fact]
        public void DynoMainViewModelTest()
        {
            // gibt es bei Maui nicht
        }

        /// <summary>
        /// Dynoes the runtime view model test.
        /// </summary>
        [Fact]
        public void DynoRuntimeViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Dyno.RuntimeViewModel>();
            vm.ResetAccelerationCommand.Execute(null);
            vm.ShowSpectrogramCommand.Execute(null);
            vm.StartAccelerationCommand.Execute(null);
            vm.StopAccelerationCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the spectrogram view model test.
        /// </summary>
        [Fact]
        public void DynoSpectrogramViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Dyno.SpectrogramViewModel>();
            vm.FilterPlotCommand.Execute(null);
            vm.RefreshAudioFileCommand.Execute(null);
            vm.RefreshPlotCommand.Execute(null);
            vm.RefreshSpectrogramCommand.Execute(null);
            vm.SpecificGraphCommand.Execute(null);
        }

        /// <summary>
        /// Einlasses the kanal view model test.
        /// </summary>
        [Fact]
        public void EinlassKanalViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Einlass.KanalViewModel>();
            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Einlasses the main view model test.
        /// </summary>
        [Fact]
        public void EinlassMainViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Einlass.MainViewModel>();
        }

        /// <summary>
        /// Einlasses the vergaser view model test.
        /// </summary>
        [Fact]
        public void EinlassVergaserViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Einlass.VergaserViewModel>();
            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Einstellungens the application view model test.
        /// </summary>
        [Fact]
        public void EinstellungenApplicationViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Einstellungen.ApplicationViewModel>();
        }


        /// <summary>
        /// Einstellungens the main view model test.
        /// </summary>
        [Fact]
        public void EinstellungenMenuViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Einstellungen.MenuViewModel>();
        }

        /// <summary>
        /// Einstellungens the vehicles view model test.
        /// </summary>
        [Fact]
        public void EinstellungenVehiclesViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Einstellungen.VehiclesViewModel>();
            vm.DeleteVehicleCommand.Execute(null);
            vm.NewVehicleCommand.Execute(null);
            vm.SaveVehicleCommand.Execute(null);
        }

        /// <summary>
        /// Homes the home view model view model test.
        /// </summary>
        [Fact]
        public void HomeHomeViewModelViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Home.HomeViewModel>();
            vm.OpenDonateCommand.Execute(null);
            vm.OpenEmailCommand.Execute(null);
            vm.OpenInstagramCommand.Execute(null);
            vm.OpenTutorialCommand.Execute(null);
            vm.OpenTwitterCommand.Execute(null);
            vm.OpenWebsiteCommand.Execute(null);
        }

        /// <summary>
        /// Mains the page view model test.
        /// </summary>
        [Fact]
        public void MainPageViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.MainPageViewModel>();
            vm.ShowHomeViewModelCommand.Execute(null);
            vm.ShowMenuViewModelCommand.Execute(null);
        }

        /// <summary>
        /// Menus the view model test.
        /// </summary>
        [Fact]
        public void MenuViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.MenuViewModel>();
            vm.ButtonCloseMenu.Execute(null);
            vm.ButtonOpenMenu.Execute(null);
        }

        /// <summary>
        /// Motors the hubraum view model test.
        /// </summary>
        [Fact]
        public void MotorHubraumViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Motor.HubraumViewModel>();
        }

        /// <summary>
        /// Motors the main view model test.
        /// </summary>
        [Fact]
        public void MotorMainViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Motor.MainViewModel>();
        }

        /// <summary>
        /// Motors the steuerdiagramm view model test.
        /// </summary>
        [Fact]
        public void MotorSteuerdiagrammViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Motor.SteuerdiagrammViewModel>();
            vm.InsertReferenceCommand.Execute(null);
            vm.InsertVehicleCommand.Execute(null);
        }

        /// <summary>
        /// Motors the umrechnung view model test.
        /// </summary>
        [Fact]
        public void MotorUmrechnungViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Motor.UmrechnungenViewModel>();
            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Motors the verdichtung view model test.
        /// </summary>
        [Fact]
        public void MotorVerdichtungViewModelTest()
        {
            var vm = new Mock<Core.ViewModels.Motor.VerdichtungViewModel>();
            vm.InsertDataCommand.Execute(null);
        }
        */
    }
}