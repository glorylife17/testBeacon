using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using testBeacon.Droid.Services;

namespace testBeacon.Droid
{
    [Activity(Label = "testBeacon", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        /// <summary>
        /// 需要開放的權限
        /// </summary>
        readonly string[] PermissionItems =
        {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            checkPermission();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            new BeaconService(this);
            LoadApplication(new App());
        }

        /// <summary>
        /// 確認 SDK 版本是否 >23 再決定 Permission 視窗是否開啟
        /// 開啟 Permission 視窗，由使用者決定
        /// </summary>
        private void checkPermission()
        {
            if ((int)Build.VERSION.SdkInt < 23) return;
            var pass = true;

            foreach (var permission in PermissionItems)
            {
                //是否取得權限
                if (CheckSelfPermission(permission) != Permission.Granted)
                {
                    pass = false;
                    break;
                }
            }

            if (!pass)
                ActivityCompat.RequestPermissions(this, PermissionItems, (int)Permission.Granted);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
