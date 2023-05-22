using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using LAMA.Services;
using Xamarin.Forms;
using AndroidX.AppCompat.App;

using Bitmap = Android.Graphics.Bitmap;
using Color = Android.Graphics.Color;
using System.IO;
using Xamarin.Essentials;

namespace LAMA.Droid
{
    [Activity(Label = "LAMA", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        Intent serviceIntent;
        private const int RequestCode = 5469;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            serviceIntent = new Intent(this, typeof(AndroidLocationService));
            SetServiceMethods();

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void SetServiceMethods()
        {
            MessagingCenter.Subscribe<StartServiceMessage>(this, "ServiceStarted",
                (StartServiceMessage message) =>
                {
                    if (!GetLocationService.IsRunning)
                    {
                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                            StartForegroundService(serviceIntent);
                        else
                            StartService(serviceIntent);
                    }
                });

            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped",
                (StopServiceMessage message) =>
                {
                    System.Diagnostics.Debug.WriteLine("WTF");
                    if (GetLocationService.IsRunning)
                    {
                        StopService(serviceIntent);
                        GetLocationService.Stop();
                        Context context = global::Android.App.Application.Context;
                    }
                    
                });

            
        }

        private bool IsServiceRunning(System.Type cls)
        {
            ActivityManager manager = (ActivityManager)GetSystemService(Context.ActivityService);
            //*/
            foreach (var service in manager.GetRunningServices(int.MinValue))
            {
                if (service.Service.ClassName.Equals(Java.Lang.Class.FromType(cls).CanonicalName))
                    return true;
            }
            //*/

            return false;
        }
    }
}