using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using SimTuning.Core.Services;
using SimTuning.Maui.UI.ViewModels;
using SimTuning.Data;
using SimTuning.Maui.UI.Services;
using SimTuning.Maui.UI.Views;

namespace SimTuning.Maui.App
{
    public partial class App : Application
    {
        private bool _initialized;

        public App()
        {
            InitializeComponent();

            // Register services
            if (!_initialized)
            {
                _initialized = true;
                Ioc.Default.ConfigureServices(
                    new ServiceCollection()
                    .AddLogging()

                    .AddSingleton<DatabaseContext, DatabaseContext>()
                    .AddSingleton<IVehicleService, VehicleService>()
                    .AddSingleton<IBrowserService, BrowserService>()
                    .AddSingleton<INavigationService, NavigationService>()


                    .AddTransient<MainPageViewModel>()

                    .AddTransient<Maui.UI.ViewModels.Home.HomeViewModel>()

                    .AddTransient<Maui.UI.ViewModels.Auslass.MainViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Auslass.AnwendungViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Auslass.TheorieViewModel>()

                    .AddTransient<Maui.UI.ViewModels.Einlass.MainViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Einlass.KanalViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Einlass.VergaserViewModel>()

                    .AddTransient<Maui.UI.ViewModels.Motor.MainViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Motor.HubraumViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Motor.SteuerdiagrammViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Motor.UmrechnungViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Motor.VerdichtungViewModel>()

                    .AddTransient<Maui.UI.ViewModels.Einstellungen.MenuViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Einstellungen.ApplicationViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Einstellungen.VehiclesViewModel>()

                    .AddTransient<Maui.UI.ViewModels.Dyno.MainViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Dyno.AudioPlayerViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Dyno.AusrollenViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Dyno.DataViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Dyno.DiagnosisViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Dyno.GeschwindigkeitViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Dyno.RuntimeViewModel>()
                    .AddTransient<Maui.UI.ViewModels.Dyno.SpectrogramViewModel>()

                    .BuildServiceProvider());
            }

            MainPage = new SimTuning.Maui.UI.Views.MainPage();
        }
    }
}