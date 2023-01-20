using Android.Util;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Sharpnado.Tabs;
using SimTuning.Core.Services;
using SimTuning.Data;
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
                .UseSkiaSharp(true)
                .UseSharpnadoTabs(loggerEnable: false)
                .UseMauiCommunityToolkit()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(filename: "MaterialIcons-Regular.ttf", alias: "MaterialDesignIcons");
                    fonts.AddFont(filename: "Roboto-Regular.ttf", alias: "Roboto-Regular");
                    fonts.AddFont(filename: "Roboto-Medium.ttf", alias: "Roboto-Medium");
                })
                .Services

                // Singleton => static registration => same instance
                .AddSingleton<DatabaseContext, DatabaseContext>()
                .AddSingleton<IVehicleService, VehicleService>()

                // Transient => created every time
                // ViewModels
                .AddTransient<Core.ViewModels.MainPageViewModel>()
                .AddTransient<Core.ViewModels.MenuViewModel>()

                .AddTransient<Core.ViewModels.Home.HomeViewModel>()

                .AddTransient<Core.ViewModels.Auslass.MainViewModel>()
                .AddTransient<Core.ViewModels.Auslass.AnwendungViewModel>()
                .AddTransient<Core.ViewModels.Auslass.TheorieViewModel>()

                .AddTransient<Core.ViewModels.Einlass.MainViewModel>()
                .AddTransient<Core.ViewModels.Einlass.KanalViewModel>()
                .AddTransient<Core.ViewModels.Einlass.VergaserViewModel>()

                .AddTransient<Core.ViewModels.Motor.MainViewModel>()
                .AddTransient<Core.ViewModels.Motor.HubraumViewModel>()
                .AddTransient<Core.ViewModels.Motor.SteuerdiagrammViewModel>()
                .AddTransient<Core.ViewModels.Motor.UmrechnungViewModel>()
                .AddTransient<Core.ViewModels.Motor.VerdichtungViewModel>()

                .AddTransient<Core.ViewModels.Einstellungen.MenuViewModel>()
                .AddTransient<Core.ViewModels.Einstellungen.ApplicationViewModel>()
                .AddTransient<Core.ViewModels.Einstellungen.VehiclesViewModel>()

                .AddTransient<Core.ViewModels.Dyno.MainViewModel>()
                .AddTransient<Core.ViewModels.Dyno.AudioPlayerViewModel>()
                .AddTransient<Core.ViewModels.Dyno.AusrollenViewModel>()
                .AddTransient<Core.ViewModels.Dyno.DataViewModel>()
                .AddTransient<Core.ViewModels.Dyno.DiagnosisViewModel>()
                .AddTransient<Core.ViewModels.Dyno.GeschwindigkeitViewModel>()
                .AddTransient<Core.ViewModels.Dyno.RuntimeViewModel>()
                .AddTransient<Core.ViewModels.Dyno.SpectrogramViewModel>()

                // logger
                .Logging.AddSerilog(dispose: true);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void SetupSerilog()
        {
            var flushInterval = new TimeSpan(0, 0, 1);
            var file = Path.Combine(FileSystem.AppDataDirectory, "MyApp.log");

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(file, flushToDiskInterval: flushInterval, encoding: System.Text.Encoding.UTF8, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 22)
            .CreateLogger();
        }
    }
}