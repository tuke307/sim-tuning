using CommunityToolkit.Maui;
using Serilog;
using Serilog.Events;
using Sharpnado.Tabs;
using SimTuning.Core;
using SimTuning.Core.Services;
using SimTuning.Data;
using SimTuning.Maui.UI.Services;
using SimTuning.Maui.UI.Views.Popups;
using SimTuning.Maui.UI.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SimTuning.Maui.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            SetupSerilog();

            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseSharpnadoTabs(loggerEnable: true, debugLogEnable: true)
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(filename: "materialdesignicons-webfont.ttf", alias: "MaterialDesignIcons");
                    fonts.AddFont(filename: "Roboto-Regular.ttf", alias: "Roboto-Regular");
                    fonts.AddFont(filename: "Roboto-Bold.ttf", alias: "Roboto-Bold");
                });

            RegisterServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterPopups(builder.Services);

            return builder.Build();
        }

        private static void SetupSerilog()
        {
            var flushInterval = new TimeSpan(0, 0, 1);
            var file = GeneralSettings.LogFilePath;

            Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.File(file, flushToDiskInterval: flushInterval, encoding: System.Text.Encoding.UTF8, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 22)
            .CreateLogger();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // Register logging
            services.AddLogging(logging => logging.AddSerilog(dispose: true));

            // Register core services
            services.AddSingleton<DatabaseContext>();
            services.AddSingleton<IVehicleService, VehicleService>();
            services.AddSingleton<IBrowserService, BrowserService>();
            services.AddSingleton<INavigationService, NavigationService>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            // Register ViewModels
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<VehiclesViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<PortTimingViewModel>();
            services.AddTransient<VehiclesViewModelBase>();

            services.AddTransient<AuslassMainViewModel>();
            services.AddTransient<AuslassAnwendungViewModel>();
            services.AddTransient<AuslassTheorieViewModel>();

            services.AddTransient<EinlassMainViewModel>();
            services.AddTransient<EinlassKanalViewModel>();
            services.AddTransient<EinlassVergaserViewModel>();

            services.AddTransient<MotorMainViewModel>();
            services.AddTransient<MotorHubraumViewModel>();
            services.AddTransient<MotorSteuerdiagrammViewModel>();
            services.AddTransient<MotorUmrechnungViewModel>();
            services.AddTransient<MotorVerdichtungViewModel>();

            services.AddTransient<EinstellungenViewModel>();

            services.AddTransient<DynoMainViewModel>();
            services.AddTransient<DynoAusrollenViewModel>();
            services.AddTransient<DynoDataViewModel>();
            services.AddTransient<DynoDiagnosisViewModel>();
            services.AddTransient<DynoGeschwindigkeitViewModel>();
            services.AddTransient<DynoRuntimeViewModel>();
            services.AddTransient<DynoAudioViewModel>();
        }

        private static void RegisterPopups(IServiceCollection services)
        {
            // Register popup view models and views
            services.AddTransientPopup<VehiclePopup, VehiclesViewModel>();
            services.AddTransientPopup<DynoCreationPopup, VehiclesViewModel>();
            services.AddTransientPopup<PortTimingPopup, PortTimingViewModel>();
            services.AddTransientPopup<EnvironmentPopup, PortTimingViewModel>();
        }
    }
}
