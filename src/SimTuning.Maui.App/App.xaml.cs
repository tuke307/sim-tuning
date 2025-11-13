using CommunityToolkit.Mvvm.DependencyInjection;

namespace SimTuning.Maui.App
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Configure Ioc.Default to use the MAUI service provider
            // This bridges the MVVM Toolkit's Ioc with MAUI's built-in DI container
            Ioc.Default.ConfigureServices(serviceProvider);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new SimTuning.Maui.UI.Views.MainPage());
        }
    }
}
