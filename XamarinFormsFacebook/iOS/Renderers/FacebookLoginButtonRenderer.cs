using System;
using System.Diagnostics;
using System.Linq;
using Facebook.CoreKit;
using Facebook.LoginKit;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinFormsFacebook;
using XamarinFormsFacebook.iOS;

[assembly: ExportRenderer(typeof(FacebookLoginButton), typeof(FacebookLoginButtonRenderer))]
namespace XamarinFormsFacebook.iOS
{
	public class FacebookLoginButtonRenderer : ViewRenderer<FacebookLoginButton, LoginButton>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<FacebookLoginButton> e)
		{
			base.OnElementChanged(e);
			if (Element == null)
				return;

			var element = Element;

			var token = AccessToken.CurrentAccessToken;
			element.LoggedIn = token != null;


			var control = new LoginButton();
			control.ReadPermissions = element.ReadPermissions;
			control.PublishPermissions = element.PublishPermissions;
			control.LoginBehavior = LoginBehavior.Browser;

			SetNativeControl(control);

			control.Completed += (s, ea) =>
			{
				var accessToken = AccessToken.CurrentAccessToken;
				var t = new GraphRequest("me", new NSDictionary("fields", "id, first_name, email, last_name, picture.width(1000).height(1000)"));
				t.Start((GraphRequestConnection connection, Foundation.NSObject result, Foundation.NSError error) =>
				{
					var graphObject = result as NSDictionary;

					var id = string.Empty;
					var first_name = string.Empty;
					var email = string.Empty;
					var last_name = string.Empty;
					var url = string.Empty;

					try
					{
						id = graphObject.ValueForKey(new NSString("id"))?.ToString();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}

					try
					{
						first_name = graphObject.ValueForKey(new NSString("first_name"))?.ToString();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}

					try
					{
						email = graphObject.ValueForKey(new NSString("email"))?.ToString();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}

					try
					{
						last_name = graphObject.ValueForKey(new NSString("last_name"))?.ToString();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}

					try
					{
						url = ((graphObject.ValueForKey(new NSString("picture")) as NSDictionary).ValueForKey(new NSString("data")) as NSDictionary).ValueForKey(new NSString("url")).ToString();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}

					element.SendShowingLoggedInUser(
							new FacebookUser(id, accessToken.TokenString, first_name, last_name, email, url)
					);
				});
			};

			control.LoggedOut += (s, ea) =>
			{
				element.SendShowingLoggedOutUser();
			};

			if (element.PublishPermissions != null)
				control.PublishPermissions = element.PublishPermissions;
			if (element.ReadPermissions != null)
				control.ReadPermissions = element.ReadPermissions;

			MessagingCenter.Subscribe(this, "Login", (s) =>
			{
				Login();
			}, element);

			MessagingCenter.Subscribe(this, "Logout", (s) =>
			{
				Logout();
			}, element);

			if (element.LoggedIn)
			{
				var accessToken = AccessToken.CurrentAccessToken;
				var t = new GraphRequest("me", null);
				t.Start((GraphRequestConnection connection, Foundation.NSObject result, Foundation.NSError error) =>
				{
					var graphObject = result as NSDictionary;
					var id = graphObject.ObjectForKey(new NSString("id")).ToString();

					element.SendShowingLoggedInUser(
						new FacebookUser(id, accessToken.TokenString, graphObject.ObjectForKey(new NSString("name")).ToString(), graphObject.ObjectForKey(new NSString("name")).ToString(), graphObject.ObjectForKey(new NSString("name")).ToString(), string.Format("http://graph.facebook.com/{0}/picture?type=large", id))
					);
				});
			}
		}

		void Login()
		{
			var session = AccessToken.CurrentAccessToken;
			if (session != null)
				return;

			var button = Control.Subviews.Select(x => x as UIButton).FirstOrDefault(x => x != null);
			if (button == null)
				throw new Exception("cannot find FB login button");
			button.SendActionForControlEvents(UIControlEvent.TouchUpInside);
		}

		void Logout()
		{
			var session = AccessToken.CurrentAccessToken;
			if (session == null)
				return;

			LoginManager manager = new LoginManager();
			manager.LogOut();
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == FacebookLoginButton.ReadPermissionsProperty.PropertyName)
				Control.ReadPermissions = Element.ReadPermissions;
			else if (e.PropertyName == FacebookLoginButton.PublishPermissionsProperty.PropertyName)
				Control.PublishPermissions = Element.PublishPermissions;
		}
	}
}

