using CommunityToolkit.Maui.Views;

namespace SimTuning.Maui.UI.Views.Popups
{
    public partial class PopupPage : Popup
    {
        public PopupPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}