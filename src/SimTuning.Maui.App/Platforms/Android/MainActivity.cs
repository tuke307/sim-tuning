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
        //public override void OnConfigurationChanged(Configuration newConfig)
        //{
        //    base.OnConfigurationChanged(newConfig);
        //    this.UpdateTheme(newConfig);
        //}

        ///// <summary>
        ///// Called when [resume].
        ///// </summary>
        //protected override void OnResume()
        //{
        //    base.OnResume();
        //    UpdateTheme(/*Resources.Configuration*/);
        //}

        ///// <summary>
        ///// Called when [start].
        ///// </summary>
        //protected override void OnStart()
        //{
        //    base.OnStart();
        //    this.UpdateTheme(/*Resources.Configuration*/);
        //}

        //private void UpdateTheme(Configuration newConfig)
        //{
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.Froyo)
        //    {
        //        var uiModeFlags = newConfig.UiMode & UiMode.NightMask;
        //        //switch (uiModeFlags)
        //        //{
        //        //    case UiMode.NightYes:
        //        //        Mvx.IoCProvider.Resolve<IThemeService>().UpdateTheme(BaseTheme.Dark);
        //        //        break;

        //        //    case UiMode.NightNo:
        //        //        Mvx.IoCProvider.Resolve<IThemeService>().UpdateTheme(BaseTheme.Light);
        //        //        break;

        //        //    default:
        //        //        throw new NotSupportedException($"UiMode {uiModeFlags} not supported");
        //        //}
        //    }
        //}
    }
}