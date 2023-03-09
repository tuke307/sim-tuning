using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels.Dyno;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoMainView : ContentPage
    {
        public DynoMainView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<MainViewModel>();
        }

        public MainViewModel ViewModel => (MainViewModel)BindingContext;
    }
}