using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Facebook;
using Xamarin.Facebook.AppEvents;
using Plugin.Permissions;
using Xamarin.Forms;

namespace XamarinFormsFacebook.Droid
{
	[Activity(Label = "XamarinFormsFacebook.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{

		public static ICallbackManager CallbackManager { get; private set; }

		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			FacebookSdk.SdkInitialize(this);
			AppEventsLogger.ActivateApp(this);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			DependencyService.Register<IFacebookManager, Droid_FacebookManager>();

			if (CallbackManager == null)
			{
				CallbackManager = CallbackManagerFactory.Create();
			}

			LoadApplication(new App());
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if(CallbackManager != null)
				CallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
			var manager = DependencyService.Get<IFacebookManager>();
			if (manager != null)
			{
				(manager as Droid_FacebookManager).CallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}
