using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;

namespace SimTuning.Maui.App
{
    [Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
    LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}