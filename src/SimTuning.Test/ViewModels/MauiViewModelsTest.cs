// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using Moq;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using Xunit;

namespace SimTuning.Test
{
    public class MauiViewModelsTest : IViewModelTest
    {
        private readonly Mock<INavigationService> navigationServiceMock =
            new Mock<INavigationService>();

        private readonly Mock<IVehicleService> vehicleServiceMock =
            new Mock<IVehicleService>();
        
        private readonly Mock<IBrowserService> browserServiceMock =
            new Mock<IBrowserService>();

        /// <summary>
        /// AuslassMainViewModelTest.
        /// </summary>
        [Fact]
        public void AuslassMainViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Auslass.MainViewModel>>();
            var vm = new Maui.UI.ViewModels.Auslass.MainViewModel(
                logger.Object);
        }

        /// <summary>
        /// AuslassAnwendungViewModelTest.
        /// </summary>
        [Fact]
        public void AuslassAnwendungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Auslass.AnwendungViewModel>>();
            var vm = new Maui.UI.ViewModels.Auslass.AnwendungViewModel(
                logger.Object,
                vehicleServiceMock.Object);

            // Act
            vm.CalculateCommand.Execute(null);
            vm.DiffusorStageCommand.Execute(null);
            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Auslasses the theorie view model test.
        /// </summary>
        [Fact]
        public void AuslassTheorieViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Auslass.TheorieViewModel>>();
            var vm = new Maui.UI.ViewModels.Auslass.TheorieViewModel(
                logger.Object,
                vehicleServiceMock.Object);

            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the audio player view model test.
        /// </summary>
        [Fact]
        public void DynoAudioPlayerViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.AudioPlayerViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.AudioPlayerViewModel(
                logger.Object,
                navigationServiceMock.Object);

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
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.AusrollenViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.AusrollenViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

            vm.RefreshPlotCommand.Execute(null);
            vm.ShowDiagnosisCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the beschleunigung view model test.
        /// </summary>
        [Fact]
        public void DynoBeschleunigungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.GeschwindigkeitViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.GeschwindigkeitViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

            vm.RefreshPlotCommand.Execute(null);
            vm.ShowAusrollenCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the data view model test.
        /// </summary>
        [Fact]
        public void DynoDataViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.DataViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.DataViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

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
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.DiagnosisViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.DiagnosisViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

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
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.MainViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.MainViewModel(
                logger.Object,
                navigationServiceMock.Object);
        }

        /// <summary>
        /// Dynoes the runtime view model test.
        /// </summary>
        [Fact]
        public void DynoRuntimeViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.RuntimeViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.RuntimeViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

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
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Dyno.SpectrogramViewModel>>();
            var vm = new Maui.UI.ViewModels.Dyno.SpectrogramViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

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
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Einlass.KanalViewModel>>();
            var vm = new Maui.UI.ViewModels.Einlass.KanalViewModel(
                logger.Object,
                vehicleServiceMock.Object);

            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Einlasses the main view model test.
        /// </summary>
        [Fact]
        public void EinlassMainViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Einlass.MainViewModel>>();
            var vm = new Maui.UI.ViewModels.Einlass.MainViewModel(
                logger.Object);
        }

        /// <summary>
        /// Einlasses the vergaser view model test.
        /// </summary>
        [Fact]
        public void EinlassVergaserViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Einlass.VergaserViewModel>>();
            var vm = new Maui.UI.ViewModels.Einlass.VergaserViewModel(
                logger.Object,
                vehicleServiceMock.Object);

            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Einstellungens the application view model test.
        /// </summary>
        [Fact]
        public void EinstellungenApplicationViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Einstellungen.ApplicationViewModel>>();
            var vm = new Maui.UI.ViewModels.Einstellungen.ApplicationViewModel(
                logger.Object,
                navigationServiceMock.Object);
        }


        /// <summary>
        /// Einstellungens the main view model test.
        /// </summary>
        [Fact]
        public void EinstellungenMenuViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Einstellungen.MenuViewModel>>();
            var vm = new Maui.UI.ViewModels.Einstellungen.MenuViewModel(
                logger.Object,
                navigationServiceMock.Object);
        }

        /// <summary>
        /// Einstellungens the vehicles view model test.
        /// </summary>
        [Fact]
        public void EinstellungenVehiclesViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Einstellungen.VehiclesViewModel>>();
            var vm = new Maui.UI.ViewModels.Einstellungen.VehiclesViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

            vm.DeleteVehicleCommand.Execute(null);
            vm.NewVehicleCommand.Execute(null);
            vm.SaveVehicleCommand.Execute(null);
        }

        /// <summary>
        /// Homes the home view model view model test.
        /// </summary>
        [Fact]
        public void HomeViewModelViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Home.HomeViewModel>>();
            var vm = new Maui.UI.ViewModels.Home.HomeViewModel(
                logger.Object,
                navigationServiceMock.Object,
                browserServiceMock.Object);

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
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.MainPageViewModel>>();
            var vm = new Maui.UI.ViewModels.MainPageViewModel(
                logger.Object,
                navigationServiceMock.Object);
        }

        /// <summary>
        /// Motors the hubraum view model test.
        /// </summary>
        [Fact]
        public void MotorHubraumViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Motor.HubraumViewModel>>();
            var vm = new Maui.UI.ViewModels.Motor.HubraumViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);
        }

        /// <summary>
        /// Motors the main view model test.
        /// </summary>
        [Fact]
        public void MotorMainViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Motor.MainViewModel>>();
            var vm = new Maui.UI.ViewModels.Motor.MainViewModel(
                logger.Object,
                navigationServiceMock.Object);
        }

        /// <summary>
        /// Motors the steuerdiagramm view model test.
        /// </summary>
        [Fact]
        public void MotorSteuerdiagrammViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Motor.SteuerdiagrammViewModel>>();
            var vm = new Maui.UI.ViewModels.Motor.SteuerdiagrammViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

            vm.InsertReferenceCommand.Execute(null);
            vm.InsertVehicleCommand.Execute(null);
        }

        /// <summary>
        /// Motors the umrechnung view model test.
        /// </summary>
        [Fact]
        public void MotorUmrechnungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Motor.UmrechnungViewModel>>();
            var vm = new Maui.UI.ViewModels.Motor.UmrechnungViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

            vm.InsertDataCommand.Execute(null);
        }

        /// <summary>
        /// Motors the verdichtung view model test.
        /// </summary>
        [Fact]
        public void MotorVerdichtungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<Maui.UI.ViewModels.Motor.VerdichtungViewModel>>();
            var vm = new Maui.UI.ViewModels.Motor.VerdichtungViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object);

            vm.InsertDataCommand.Execute(null);
        }
    }
}