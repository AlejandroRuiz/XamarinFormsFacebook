using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Facebook.CoreKit;
using Facebook.LoginKit;
using Facebook.ShareKit;
using Foundation;
using UIKit;
using Xamarin.Forms;
using XamarinFormsFacebook.Shared;

namespace XamarinFormsFacebook.iOS
{
	public class iOS_FacebookManager : IFacebookManager
	{
		public iOS_FacebookManager()
		{
		}

		#region IFacebookManager

		public Task<FacebookUser> Login()
		{
			var window = UIApplication.SharedApplication.KeyWindow;
			var vc = window.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;
			}

			var tcs = new TaskCompletionSource<FacebookUser>();
			LoginManager manager = new LoginManager();
			manager.LogOut();
			manager.LoginBehavior = LoginBehavior.SystemAccount;
			manager.LogInWithReadPermissions(new string[] { "public_profile", "email" }, vc, (result, error) =>
			{
				if (error != null || result == null || result.IsCancelled)
				{
					if (error != null)
						Debug.WriteLine(error.LocalizedDescription);
					Debug.WriteLine(error.LocalizedDescription);
					tcs.TrySetResult(null);
				}
				else
				{
					var request = new Facebook.CoreKit.GraphRequest("me", new NSDictionary("fields", "id, first_name, email, last_name, picture.width(1000).height(1000)"));
					request.Start((connection, result1, error1) =>
					{
						if (error1 != null || result1 == null)
						{
							Debug.WriteLine(error1.LocalizedDescription);
							tcs.TrySetResult(null);
						}
						else
						{
							var id = string.Empty;
							var first_name = string.Empty;
							var email = string.Empty;
							var last_name = string.Empty;
							var url = string.Empty;

							try
							{
								id = result1.ValueForKey(new NSString("id"))?.ToString();
							}
							catch (Exception e)
							{
								Debug.WriteLine(e.Message);
							}

							try
							{
								first_name = result1.ValueForKey(new NSString("first_name"))?.ToString();
							}
							catch (Exception e)
							{
								Debug.WriteLine(e.Message);
							}

							try
							{
								email = result1.ValueForKey(new NSString("email"))?.ToString();
							}
							catch (Exception e)
							{
								Debug.WriteLine(e.Message);
							}

							try
							{
								last_name = result1.ValueForKey(new NSString("last_name"))?.ToString();
							}
							catch (Exception e)
							{
								Debug.WriteLine(e.Message);
							}

							try
							{
								url = ((result1.ValueForKey(new NSString("picture")) as NSDictionary).ValueForKey(new NSString("data")) as NSDictionary).ValueForKey(new NSString("url")).ToString();
							}
							catch (Exception e)
							{
								Debug.WriteLine(e.Message);
							}
							if (tcs != null)
							{
								tcs.TrySetResult(new FacebookUser(id, result.Token.TokenString, first_name, last_name, email, url));
							}
						}
					});
				}
			});
			return tcs.Task;
		}

		public async System.Threading.Tasks.Task LogOut()
		{
			LoginManager manager = new LoginManager();
			manager.LogOut();
		}

		TaskCompletionSource<bool> tcsPhoto;

		public Task<bool> PostPhoto(ImageSource image)
		{

			var tcsPhoto = new TaskCompletionSource<bool>();

			var token = AccessToken.CurrentAccessToken?.TokenString;
			if (string.IsNullOrWhiteSpace(token))
			{
				tcsPhoto.TrySetResult(false);
			}
			else {

				bool hasRights;
				if (!AccessToken.CurrentAccessToken.HasGranted("publish_actions"))
				{
					hasRights = RequestPublishPermissions(new[] { "publish_actions" }).Result;
				}
				else
				{
					hasRights = true;
				}

				if (hasRights)
				{
					var handler = image.GetHandler();
					var nativeImage = handler.LoadImageAsync(image).Result;

					var parameters = new NSDictionary("picture", nativeImage.AsPNG());

					var request = new GraphRequest("/" + AccessToken.CurrentAccessToken.UserID + "/photos", parameters, AccessToken.CurrentAccessToken.TokenString, null, "POST");
					var requestConnection = new GraphRequestConnection();
					requestConnection.AddRequest(request, (connection, result, error) =>
					{
						if (error != null)
						{
							tcsPhoto.TrySetResult(false);
							return;
						}
						tcsPhoto.TrySetResult(true);
					});
					requestConnection.Start();
				}
				else
				{
					tcsPhoto.TrySetResult(hasRights);
				}
			}
			return tcsPhoto.Task;
		}

		public Task<bool> PostText(string message)
		{
			var tcs = new TaskCompletionSource<bool>();

			var token = AccessToken.CurrentAccessToken?.TokenString;
			if (string.IsNullOrWhiteSpace(token))
			{
				tcs.TrySetResult(false);
			}
			else
			{
				bool hasRights;
				if (!AccessToken.CurrentAccessToken.HasGranted("publish_actions"))
				{
					hasRights = RequestPublishPermissions(new[] { "publish_actions" }).Result;
				}
				else
				{
					hasRights = true;
				}

				if (hasRights)
				{
					GraphRequest request = new GraphRequest("me/feed", new NSDictionary("message", message), AccessToken.CurrentAccessToken.TokenString, null, "POST");
					var requestConnection = new GraphRequestConnection();
					requestConnection.AddRequest(request, (connection, result, error) =>
					{
						if (error != null)
						{
							tcs.TrySetResult(false);
							return;
						}

						tcs.TrySetResult(true);
					});
					requestConnection.Start();
				}
				else
				{
					tcs.TrySetResult(hasRights);
				}
			}

			return tcs.Task;
		}

		public Task<bool> SimpleLogin()
		{
			var window = UIApplication.SharedApplication.KeyWindow;
			var vc = window.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;
			}

			var tcs = new TaskCompletionSource<bool>();
			LoginManager manager = new LoginManager();
			manager.LogOut();
			manager.LoginBehavior = LoginBehavior.SystemAccount;
			manager.LogInWithReadPermissions(new string[] { "public_profile", "email" }, vc, (result, error) =>
			{
				if (error != null || result == null || result.IsCancelled)
				{
					if(error != null)
						Debug.WriteLine(error.LocalizedDescription);
					tcs.TrySetResult(false);
				}
				else
				{
					tcs.TrySetResult(true);
				}
			});
			return tcs.Task;
		}

		public async Task<bool> ValidateToken()
		{
			var token = AccessToken.CurrentAccessToken?.TokenString;
			if (string.IsNullOrWhiteSpace(token))
				return false;
			return true;
		}

		#endregion

		Task<bool> RequestPublishPermissions(string[] permissions)
		{
			var tcs = new TaskCompletionSource<bool>();
			var window = UIApplication.SharedApplication.KeyWindow;
			var vc = window.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;
			}
			var login = new LoginManager();
			login.LogInWithPublishPermissions(permissions, vc, (result, error) =>
			{
				if (error != null)
				{
					tcs.TrySetResult(false);
				}

				if (result.IsCancelled)
				{
					tcs.TrySetResult(false);
				}

				tcs.TrySetResult(true);
			});
			return tcs.Task;
		}
	}
}
