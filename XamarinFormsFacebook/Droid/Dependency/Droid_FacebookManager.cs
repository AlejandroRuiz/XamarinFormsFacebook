using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using Org.Json;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Share;
using Xamarin.Facebook.Share.Model;
using Xamarin.Forms;
using XamarinFormsFacebook.Shared;

namespace XamarinFormsFacebook.Droid
{
	public class Droid_FacebookManager : Java.Lang.Object,  IFacebookManager, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, GraphRequest.ICallback
	{
		public ICallbackManager CallbackManager;

		TaskCompletionSource<FacebookUser> tcs;

		TaskCompletionSource<bool> tcss;

		bool _isSimpleLogin;

		public Droid_FacebookManager()
		{
			CallbackManager = CallbackManagerFactory.Create();
			LoginManager.Instance.RegisterCallback(CallbackManager, this);
		}

		#region GraphRequest.ICallback

		public void OnCompleted(GraphResponse p0)
		{
			if (tcsText != null)
			{
				tcsText.TrySetResult((p0.Error == null) ? true : false);
				tcsText = null;
			}
		}

		#endregion

		#region IFacebookCallback

		public void OnCancel()
		{
			if (tcs != null)
			{
				tcs.TrySetResult(null);
				tcs = null;
			}

			if (tcss != null)
			{
				tcss.TrySetResult(false);
				tcss = null;
			}

			if (_pTask != null)
			{
				_pTask.TrySetResult(false);
				_pTask = null;
			}

			if (tcsPhoto != null)
			{
				tcsPhoto.TrySetResult(false);
				tcsPhoto = null;
			}
		}

		public void OnError(FacebookException p0)
		{
			if (tcs != null)
			{
				tcs.TrySetResult(null);
				tcs = null;
			}

			if (tcss != null)
			{
				tcss.TrySetResult(false);
				tcss = null;
			}

			if (_pTask != null)
			{
				_pTask.TrySetResult(false);
				_pTask = null;
			}

			if (tcsPhoto != null)
			{
				tcsPhoto.TrySetResult(false);
				tcsPhoto = null;
			}
		}

		public void OnSuccess(Java.Lang.Object p0)
		{
			var n = p0 as LoginResult;
			if (n != null)
			{
				if (_isPermissionsLogin && _pTask != null)
				{
					_isPermissionsLogin = false;
					_pTask.TrySetResult(true);
					_pTask = null;
					return;
				}

				if (_isSimpleLogin)
				{
					_isSimpleLogin = false;
					if (tcss != null)
					{
						tcss.TrySetResult(true);
					}
					tcss = null;
					return;
				}
				var request = GraphRequest.NewMeRequest(n.AccessToken, this);
				var bundle = new Android.OS.Bundle();
				bundle.PutString("fields", "id, first_name, email, last_name, picture.width(500).height(500)");
				request.Parameters = bundle;
				request.ExecuteAsync();
			}
			else if (p0 is SharerResult)
			{
				if (tcsPhoto != null)
				{
					tcsPhoto.TrySetResult(true);
					tcsPhoto = null;
				}
			}
		}

		#endregion

		#region GraphRequest.IGraphJSONObjectCallback

		public void OnCompleted(JSONObject p0, GraphResponse p1)
		{
			var id = string.Empty;
			var first_name = string.Empty;
			var email = string.Empty;
			var last_name = string.Empty;
			var pictureUrl = string.Empty;

			if (p0.Has("id"))
				id = p0.GetString("id");

			if (p0.Has("first_name"))
				first_name = p0.GetString("first_name");

			if (p0.Has("email"))
				email = p0.GetString("email");

			if (p0.Has("last_name"))
				last_name = p0.GetString("last_name");

			if (p0.Has("picture"))
			{
				var p2 = p0.GetJSONObject("picture");
				if (p2.Has("data"))
				{
					var p3 = p2.GetJSONObject("data");
					if (p3.Has("url"))
					{
						pictureUrl = p3.GetString("url");
					}
				}
			}

			if (tcs != null)
			{
				tcs.TrySetResult(new FacebookUser(id, AccessToken.CurrentAccessToken.Token, first_name, first_name, email, pictureUrl));
			}
		}

		#endregion

		#region IFacebookManager

		public Task<FacebookUser> Login()
		{
			_isSimpleLogin = false;

			if (tcs != null)
			{
				tcs.TrySetResult(null);
				tcs = null;
			}
			tcs = new TaskCompletionSource<FacebookUser>();
			LoginManager.Instance.SetLoginBehavior(LoginBehavior.NativeWithFallback);
			LoginManager.Instance.LogInWithReadPermissions(Xamarin.Forms.Forms.Context as Activity, new List<string> { "public_profile", "email" });
			return tcs.Task;
		}

		public async Task LogOut()
		{
			LoginManager.Instance.LogOut();
		}

		TaskCompletionSource<bool> tcsPhoto;

		public Task<bool> PostPhoto(ImageSource image)
		{
			if (tcsPhoto != null)
			{
				tcsPhoto.TrySetResult(false);
				tcsPhoto = null;
			}
			tcsPhoto = new TaskCompletionSource<bool>();

			var token = AccessToken.CurrentAccessToken?.Token;
			if (string.IsNullOrWhiteSpace(token))
			{
				tcsPhoto.TrySetResult(false);
			}
			else {

				bool hasRights;
				if (!HasPublishPermission())
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
					var nativeImage = handler.LoadImageAsync(image, Xamarin.Forms.Forms.Context).Result;

					var sharePhoto = new SharePhoto.Builder().SetBitmap(nativeImage).Build().JavaCast<SharePhoto>();

					var photos = new List<SharePhoto>();
					photos.Add(sharePhoto);

					var sharePhotoContent = new SharePhotoContent.Builder()
						.SetPhotos(photos).Build();

					ShareApi.Share(sharePhotoContent, this);
				}
				else
				{
					tcsPhoto.TrySetResult(hasRights);
				}
			}
			return tcsPhoto.Task;	
					
		}

		TaskCompletionSource<bool> tcsText;

		public Task<bool> PostText(string message)
		{
			if (tcsText != null)
			{
				tcsText.TrySetResult(false);
				tcsText = null;
			}

			tcsText = new TaskCompletionSource<bool>();

			var token = AccessToken.CurrentAccessToken?.Token;
			if (string.IsNullOrWhiteSpace(token))
			{
				tcsText.TrySetResult(false);
			}
			else {

				bool hasRights;
				if (!HasPublishPermission())
				{
					hasRights = RequestPublishPermissions(new[] { "publish_actions" }).Result;
				}
				else
				{
					hasRights = true;
				}

				if (hasRights)
				{
					//TODO Login here
					var bundle = new Bundle();
					bundle.PutString("message", message);
					GraphRequest request = new GraphRequest(AccessToken.CurrentAccessToken, "me/feed", bundle, HttpMethod.Post, this);
					request.ExecuteAsync();
				}
				else
				{
					tcsText.TrySetResult(hasRights);
				}
			}
			return tcsText.Task;
		}

		public Task<bool> SimpleLogin()
		{
			_isSimpleLogin = true;

			if (tcss != null)
			{
				tcss.TrySetResult(false);
				tcss = null;
			}
			tcss = new TaskCompletionSource<bool>();
			LoginManager.Instance.SetLoginBehavior(LoginBehavior.NativeWithFallback);
			LoginManager.Instance.LogInWithReadPermissions(Xamarin.Forms.Forms.Context as Activity, new List<string> { "public_profile", "email" });
			return tcss.Task;
		}

		public async Task<bool> ValidateToken()
		{
			var token = AccessToken.CurrentAccessToken?.Token;
			if (string.IsNullOrWhiteSpace(token))
				return false;
			return true;
		}


		#endregion

		bool HasPublishPermission()
		{
			var accessToken = AccessToken.CurrentAccessToken;
			return accessToken != null && accessToken.Permissions.Contains("publish_actions");
		}

		bool _isPermissionsLogin;
		TaskCompletionSource<bool> _pTask;

		Task<bool> RequestPublishPermissions(string[] permissions)
		{
			_isPermissionsLogin = true;
			if (_pTask != null)
			{
				_pTask.TrySetResult(false);
				_pTask = null;
			}
			
			_pTask = new TaskCompletionSource<bool>();
			LoginManager.Instance.SetLoginBehavior(LoginBehavior.NativeWithFallback);
			LoginManager.Instance.LogInWithPublishPermissions(Xamarin.Forms.Forms.Context as Activity, new List<string> { "publish_actions" });
			return _pTask.Task;
		}
	}
}
