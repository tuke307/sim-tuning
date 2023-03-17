using CommunityToolkit.Maui;
using Serilog;
using Serilog.Events;
using Sharpnado.Tabs;
using SimTuning.Core;
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
                .UseSkiaSharp(registerRenderers: true)
                .UseSharpnadoTabs(loggerEnable: true, debugLogEnable: true)
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(filename: "materialdesignicons-webfont.ttf", alias: "MaterialDesignIcons");
                    fonts.AddFont(filename: "Roboto-Regular.ttf", alias: "Roboto-Regular");
                    fonts.AddFont(filename: "Roboto-Bold.ttf", alias: "Roboto-Bold");
                });

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
    }
}