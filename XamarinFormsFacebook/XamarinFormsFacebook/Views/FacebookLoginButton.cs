using System;
using Xamarin.Forms;

namespace XamarinFormsFacebook
{
	public class FacebookLoginButton : View
	{
		public static readonly BindableProperty ReadPermissionsProperty = BindableProperty.Create(
			propertyName: "ReadPermissions",
			returnType: typeof(string[]),
			declaringType: typeof(FacebookLoginButton),
			defaultValue: null
		);

		public string[] ReadPermissions
		{
			get
			{
				return (string[])GetValue(ReadPermissionsProperty);
			}
			set
			{
				SetValue(ReadPermissionsProperty, value);
			}
		}

		public static readonly BindableProperty PublishPermissionsProperty = BindableProperty.Create(
			propertyName: "PublishPermissions",
			returnType: typeof(string[]),
			declaringType: typeof(FacebookLoginButton),
			defaultValue: null
		);
		public string[] PublishPermissions
		{
			get
			{
				return (string[])GetValue(PublishPermissionsProperty);
			}
			set
			{
				SetValue(PublishPermissionsProperty, value);
			}
		}

		public static readonly BindableProperty LoggedInProperty = BindableProperty.Create(
			propertyName: "LoggedIn",
			returnType: typeof(bool),
			declaringType: typeof(FacebookLoginButton),
			defaultValue: false,
			defaultBindingMode: BindingMode.OneWayToSource
		);

		public bool LoggedIn
		{
			get
			{
				return (bool)GetValue(LoggedInProperty);
			}
			set
			{
				SetValue(LoggedInProperty, value);
			}
		}

		public event EventHandler<FacebookLoginEventArgs> ShowingLoggedInUser;
		public event EventHandler ShowingLoggedOutUser;

		public void SendShowingLoggedInUser(FacebookUser userData)
		{
			LoggedIn = true;
			var handler = ShowingLoggedInUser;
			if (handler != null)
				handler(this, new FacebookLoginEventArgs { User = userData });
		}

		public void SendShowingLoggedOutUser()
		{
			LoggedIn = false;
			var handler = ShowingLoggedOutUser;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		public void Login()
		{
			MessagingCenter.Send(this, "Login");
		}

		public void Logout()
		{
			MessagingCenter.Send(this, "Logout");
		}
	}
}
