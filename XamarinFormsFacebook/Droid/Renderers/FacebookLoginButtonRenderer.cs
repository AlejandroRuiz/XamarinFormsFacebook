using System;
using System.Threading.Tasks;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinFormsFacebook;
using XamarinFormsFacebook.Droid;

[assembly: ExportRenderer(typeof(FacebookLoginButton), typeof(FacebookLoginButtonRenderer))]
namespace XamarinFormsFacebook.Droid
{
	public class FacebookLoginButtonRenderer : ViewRenderer<FacebookLoginButton, LoginButton>, IFacebookCallback
	{
		

		public FacebookLoginButtonRenderer()
		{
		}

		class CustomAccessTokenTracker : AccessTokenTracker
		{
			FacebookLoginButton Element;

			public CustomAccessTokenTracker(FacebookLoginButton element)
			{
				Element = element;
			}

			protected override void OnCurrentAccessTokenChanged(AccessToken oldAccessToken, AccessToken currentAccessToken)
			{
				if (Element != null)
				{
					var session = Xamarin.Facebook.AccessToken.CurrentAccessToken;
					if (session == null)
					{
						Device.BeginInvokeOnMainThread(() =>
						{
							Element.SendShowingLoggedOutUser();
						});
					}
				}
			}
		}

		#region IFacebookCallback

		public void OnCancel()
		{
			Element.SendShowingLoggedOutUser();
		}

		public void OnError(FacebookException error)
		{
			Element.SendShowingLoggedOutUser();
		}

		public void OnSuccess(Java.Lang.Object result)
		{
			LogIn();
		}

		#endregion

		private AccessTokenTracker fbTracker;

		Task LogIn()
		{
			return Task.Run(() =>
			{
				var session = Xamarin.Facebook.AccessToken.CurrentAccessToken;
				if (session != null)
				{
					var request = new GraphRequest(session, "me");
					var response = request.ExecuteAndWait();
					var id = response.JSONObject.GetString("id");
					var name = response.JSONObject.GetString("name");
					Device.BeginInvokeOnMainThread(() =>
					{
						Element.SendShowingLoggedInUser(
							new FacebookUser(id, session.Token, name, name, name, string.Format("http://graph.facebook.com/{0}/picture?type=large", id))
						);
					});
				}
			});
		}

		protected override void OnElementChanged(ElementChangedEventArgs<FacebookLoginButton> ec)
		{
			base.OnElementChanged(ec);

			var element = ec.NewElement;
			{
				var session = Xamarin.Facebook.AccessToken.CurrentAccessToken;
				element.LoggedIn = session != null;
			}

			var control = new LoginButton(base.Context);
			control.RegisterCallback(MainActivity.CallbackManager, this);
			LoginManager.Instance.RegisterCallback(MainActivity.CallbackManager, this);

			fbTracker = new CustomAccessTokenTracker(base.Element)
			{

			};
			fbTracker.StartTracking();

			if (element.ReadPermissions != null)
				control.SetReadPermissions(element.ReadPermissions);
			else {
				if (element.PublishPermissions != null)
					control.SetPublishPermissions(element.PublishPermissions);
			}

			MessagingCenter.Subscribe(this, "Login", (s) =>
			{
				Login();
			}, element);

			MessagingCenter.Subscribe(this, "Logout", (s) =>
			{
				Logout();
			}, element);

			bool sentInitialEvent = false;
			control.ViewAttachedToWindow += (s, e) =>
			{
				if (!sentInitialEvent && Element != null)
				{
					sentInitialEvent = true;
					var session = Xamarin.Facebook.AccessToken.CurrentAccessToken;
					if (session != null)
					{
						LogIn();
					}
					else
						Element.SendShowingLoggedOutUser();
				}
			};

			var oldControl = Control;
			SetNativeControl(control);
			if (oldControl != null)
				oldControl.Dispose(); // collect Java Objects on Android
		}

		void Login()
		{
			var session = Xamarin.Facebook.AccessToken.CurrentAccessToken;
			if (session != null)
				return;

			// TODO: not sure if this works, might need to call it on a button subview
			Control.PerformClick();
		}

		void Logout()
		{
			var session = AccessToken.CurrentAccessToken;
			if (session == null)
				return;

			LoginManager manager = LoginManager.Instance;
			manager.LogOut();
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == FacebookLoginButton.ReadPermissionsProperty.PropertyName)
				Control.SetReadPermissions(Element.ReadPermissions);
			else if (e.PropertyName == FacebookLoginButton.PublishPermissionsProperty.PropertyName)
				Control.SetPublishPermissions(Element.PublishPermissions);
		}
	}
}

