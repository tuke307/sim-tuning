using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using SimTuning.Core.Services;
using SimTuning.Core.ViewModels;
using SimTuning.Core.ViewModels.Home;
using SimTuning.Data;
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
                    .AddTransient<MenuViewModel>()

                    .AddTransient<HomeViewModel>()

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

                    .BuildServiceProvider());
            }

            MainPage = new SimTuning.Maui.UI.Views.MainPage();
        }
    }
}