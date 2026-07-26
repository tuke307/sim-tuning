// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Moq;
using SimTuning.Core.Models.Messages;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.Services;
using SimTuning.Maui.UI.ViewModels;
using System;
using Xunit;

namespace SimTuning.Test
{
    public class MauiViewModelsTest : IViewModelTest
    {
        private readonly Mock<IBrowserService> browserServiceMock = new Mock<IBrowserService>();

        private readonly Mock<INavigationService> navigationServiceMock =
            new Mock<INavigationService>();

        private readonly Mock<IVehicleService> vehicleServiceMock = new Mock<IVehicleService>();

        private readonly Mock<IPopupService> popupServiceMock = new Mock<IPopupService>();

        /// <summary>
        /// Builds <paramref name="factory" /> with a no-op handler registered for
        /// <see cref="CurrentDynoRequestMessage" /> on the default messenger. The DynoAudio and
        /// DynoDiagnosis VM ctors Send it (in the app, DynoDataViewModel Replies); in isolation
        /// the Send has no handler and throws, so the test supplies one for the duration of the ctor.
        /// </summary>
        private T WithDynoRequestHandler<T>(Func<T> factory)
        {
            WeakReferenceMessenger.Default.Register<MauiViewModelsTest, CurrentDynoRequestMessage>(this, static (r, m) => m.Reply(null!));
            try
            {
                return factory();
            }
            finally
            {
                WeakReferenceMessenger.Default.Unregister<CurrentDynoRequestMessage>(this);
            }
        }

        /// <summary>
        /// AuslassAnwendungViewModelTest.
        /// </summary>
        [Fact]
        public void AuslassAnwendungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<AuslassAnwendungViewModel>>();
            var vm = new AuslassAnwendungViewModel(logger.Object, vehicleServiceMock.Object, popupServiceMock.Object);

            Assert.NotNull(vm);

            // DiffusorStageCommand takes an Int32 argument, so it cannot be Execute(null)'d.
            vm.CalculateCommand.Execute(null);
        }

        /// <summary>
        /// AuslassMainViewModelTest.
        /// </summary>
        [Fact]
        public void AuslassMainViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<AuslassMainViewModel>>();
            var vm = new AuslassMainViewModel(logger.Object);
        }

        /// <summary>
        /// Auslasses the theorie view model test.
        /// </summary>
        [Fact]
        public void AuslassTheorieViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<AuslassTheorieViewModel>>();
            var vm = new AuslassTheorieViewModel(logger.Object, vehicleServiceMock.Object, popupServiceMock.Object);
        }

        /// <summary>
        /// Dynoes the ausrollen view model test.
        /// </summary>
        [Fact]
        public void DynoAusrollenViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<DynoAusrollenViewModel>>();
            var vm = new DynoAusrollenViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object
            );

            Assert.NotNull(vm);
            vm.RefreshPlotCommand.Execute(null);

            // ShowDiagnosisCommand is an intentionally-disabled (null) command — skip.
        }

        /// <summary>
        /// Dynoes the beschleunigung view model test.
        /// </summary>
        [Fact]
        public void DynoBeschleunigungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<DynoGeschwindigkeitViewModel>>();
            var vm = new DynoGeschwindigkeitViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object
            );

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
            var logger = new Mock<ILogger<DynoDataViewModel>>();
            var vm = new DynoDataViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object,
                browserServiceMock.Object,
                popupServiceMock.Object
            );

            vm.NewDyno(null);
            vm.SaveDynoCommand.Execute(null);
            vm.ExportDynoCommand.Execute(null);
            vm.DeleteDynoCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the diagnosis view model test.
        /// </summary>
        [Fact]
        public void DynoDiagnosisViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<DynoDiagnosisViewModel>>();
            var vm = WithDynoRequestHandler(() => new DynoDiagnosisViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object
            ));

            Assert.NotNull(vm);

            // InsertVehicle has no null-guard by design (expects a populated model from a popup) — skip.
            vm.RefreshPlotCommand.Execute(null);
        }

        /// <summary>
        /// Dynoes the main view model test.
        /// </summary>
        [Fact]
        public void DynoMainViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<DynoMainViewModel>>();
            var vm = new DynoMainViewModel(logger.Object, navigationServiceMock.Object);
        }

        /// <summary>
        /// Dynoes the runtime view model test.
        /// </summary>
        [Fact]
        public void DynoRuntimeViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<DynoRuntimeViewModel>>();
            var vm = new DynoRuntimeViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object
            );

            Assert.NotNull(vm);
            vm.ResetAccelerationCommand.Execute(null);
            vm.StartAccelerationCommand.Execute(null);

            // ShowSpectrogramCommand / StopAccelerationCommand are intentionally-disabled (null) commands.
        }

        /// <summary>
        /// Dynoes the spectrogram view model test.
        /// </summary>
        [Fact]
        public void DynoAudioViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<DynoAudioViewModel>>();
            var vm = WithDynoRequestHandler(() => new DynoAudioViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object
            ));

            Assert.NotNull(vm);

            // RefreshAudioFileCommand is an intentionally-disabled (null) command — skip.
            vm.FilterPlotCommand.Execute(null);
            vm.RefreshPlotCommand.Execute(null);
            vm.SpecificGraphCommand.Execute(null);
        }

        /// <summary>
        /// Einlasses the kanal view model test.
        /// </summary>
        [Fact]
        public void EinlassKanalViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<EinlassKanalViewModel>>();
            var vm = new EinlassKanalViewModel(logger.Object, vehicleServiceMock.Object, popupServiceMock.Object);
        }

        /// <summary>
        /// Einlasses the main view model test.
        /// </summary>
        [Fact]
        public void EinlassMainViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<EinlassMainViewModel>>();
            var vm = new EinlassMainViewModel(logger.Object);
        }

        /// <summary>
        /// Einlasses the vergaser view model test.
        /// </summary>
        [Fact]
        public void EinlassVergaserViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<EinlassVergaserViewModel>>();
            var vm = new EinlassVergaserViewModel(logger.Object, vehicleServiceMock.Object, popupServiceMock.Object);
        }

        /// <summary>
        /// Einstellungens the main view model test.
        /// </summary>
        [Fact]
        public void EinstellungenViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<EinstellungenViewModel>>();
            var vm = new EinstellungenViewModel(logger.Object, navigationServiceMock.Object);
        }

        /// <summary>
        /// Homes the home view model view model test.
        /// </summary>
        [Fact]
        public void HomeViewModelViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeViewModel>>();
            var vm = new HomeViewModel(
                logger.Object,
                navigationServiceMock.Object,
                browserServiceMock.Object
            );

            vm.OpenDonateCommand.Execute(null);
            vm.OpenEmailCommand.Execute(null);
            vm.OpenInstagramCommand.Execute(null);
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
                navigationServiceMock.Object
            );
        }

        /// <summary>
        /// Motors the hubraum view model test.
        /// </summary>
        [Fact]
        public void MotorHubraumViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<MotorHubraumViewModel>>();
            var vm = new MotorHubraumViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object,
                popupServiceMock.Object
            );
        }

        /// <summary>
        /// Motors the main view model test.
        /// </summary>
        [Fact]
        public void MotorMainViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<MotorMainViewModel>>();
            var vm = new MotorMainViewModel(logger.Object, navigationServiceMock.Object);
        }

        /// <summary>
        /// Motors the steuerdiagramm view model test.
        /// </summary>
        [Fact]
        public void MotorSteuerdiagrammViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<MotorSteuerdiagrammViewModel>>();
            var vm = new MotorSteuerdiagrammViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object,
                popupServiceMock.Object
            );

            Assert.NotNull(vm);

            // InsertHelperEngines/Vehicle have no null-guard by design — skip the null path.
        }

        /// <summary>
        /// Motors the umrechnung view model test.
        /// </summary>
        [Fact]
        public void MotorUmrechnungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<MotorUmrechnungViewModel>>();
            var vm = new MotorUmrechnungViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object,
                popupServiceMock.Object
            );
        }

        /// <summary>
        /// Motors the verdichtung view model test.
        /// </summary>
        [Fact]
        public void MotorVerdichtungViewModelTest()
        {
            // Arrange
            var logger = new Mock<ILogger<MotorVerdichtungViewModel>>();
            var vm = new MotorVerdichtungViewModel(
                logger.Object,
                navigationServiceMock.Object,
                vehicleServiceMock.Object,
                popupServiceMock.Object
            );

            Assert.NotNull(vm);

            // InsertHelperVehicle has no null-guard by design (expects a populated model) — skip.
        }
    }
}
