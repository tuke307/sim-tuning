using CommunityToolkit.Mvvm.DependencyInjection;
using SimTuning.Maui.UI.ViewModels;

namespace SimTuning.Maui.UI.Views.Dyno
{
    public partial class DynoMainView : ContentPage
    {
        public DynoMainViewModel ViewModel => (DynoMainViewModel)BindingContext;

        public DynoMainView()
        {
            InitializeComponent();

            BindingContext = Ioc.Default.GetRequiredService<DynoMainViewModel>();
        }
    }
}