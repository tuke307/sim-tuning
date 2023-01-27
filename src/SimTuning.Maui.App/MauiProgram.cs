using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Sharpnado.Tabs;
using SimTuning.Core;
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
                .UseSkiaSharp(registerRenderers: true)
                .UseSharpnadoTabs(loggerEnable: false, debugLogEnable: true)
                .UseMauiCommunityToolkit()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(filename: "materialdesignicons-webfont.ttf", alias: "MaterialDesignIcons");
                    fonts.AddFont(filename: "Roboto-Regular.ttf", alias: "Roboto-Regular");
                    fonts.AddFont(filename: "Roboto-Medium.ttf", alias: "Roboto-Medium");
                })

                //.RegisterAppServices()
                //.RegisterViewModels()

                // logger
                .Logging.AddSerilog(dispose: true);

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
            .WriteTo.File(file, flushToDiskInterval: flushInterval, encoding: System.Text.Encoding.UTF8, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 22)
            .CreateLogger();
        }
    }
}