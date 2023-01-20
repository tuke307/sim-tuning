using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SimTuning.Core.Services;
using SimTuning.Core.ViewModels;

namespace SimTuning.Maui.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            LiveCharts.Configure(config =>
               config
                   // registers SkiaSharp as the library backend
                   // REQUIRED unless you build your own
                   .AddSkiaSharp()

                   // adds the default supported types
                   // OPTIONAL but highly recommend
                   .AddDefaultMappers()
               // .HasMap<Foo>( .... )
               // .HasMap<Bar>( .... )
               );

            //MainPage = new NavigationPage();
            //navigationService.Navigate<MainPageViewModel>();

            MainPage = new AppShell();
        }
    }
}