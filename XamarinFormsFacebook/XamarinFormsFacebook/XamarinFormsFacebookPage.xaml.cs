using System;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace XamarinFormsFacebook
{
	public partial class XamarinFormsFacebookPage : ContentPage
	{
		const string CANCEL_TEXT = "Cancel";
		const string CAMERA_TEXT = "Camera";
		const string LIBRARY_TEXT = "Library";

		void Handle_Clicked_LogOut_FB_Button(object sender, System.EventArgs e)
		{
			_fbButton.Logout();
			CleanUserData();
		}

		async void Handle_Clicked_LogIn_CustomS_Button(object sender, System.EventArgs e)
		{
			var result = await _manager.SimpleLogin();
			await DisplayAlert("App", result ? "Connected" : "Error", "Ok");
		}

		async void Handle_Clicked_LogIn_CustomD_Button(object sender, System.EventArgs e)
		{
			var result = await _manager.Login();
			if (result == null)
			{
				await DisplayAlert("App", "Error while login", "Ok");
				return;
			}
			SetFieldsValues(result);
		}

		async void Handle_Clicked_LogOut_Custom_Button(object sender, System.EventArgs e)
		{
			await _manager.LogOut();
			CleanUserData();
		}

		async void Handle_Clicked_Post_Image(object sender, System.EventArgs e)
		{
			var result = await DisplayActionSheet("App", CANCEL_TEXT, null, LIBRARY_TEXT, CAMERA_TEXT);
			if (string.IsNullOrWhiteSpace(result) || result == CANCEL_TEXT)
			{
				return;
			}

			MediaFile mFile = null;

			if (result == LIBRARY_TEXT)
			{
				if (!CrossMedia.Current.IsPickPhotoSupported)
				{
					await DisplayAlert("App", "Library is not supported in this device", "Ok");
					return;
				}
				mFile = await CrossMedia.Current.PickPhotoAsync();
			}
			else if (result == CAMERA_TEXT)
			{
				if (!CrossMedia.Current.IsCameraAvailable)
				{
					await DisplayAlert("App", "Camera is not supported in this device", "Ok");
					return;
				}
				var sOptions = new StoreCameraMediaOptions();
				sOptions.DefaultCamera = CameraDevice.Front;
				sOptions.SaveToAlbum = true;
				mFile = await CrossMedia.Current.TakePhotoAsync(sOptions);
			}

			if (mFile == null)
			{
				await DisplayAlert("App", "No media selected", "Ok");
				return;
			}

			var shareResult = await _manager.PostPhoto(mFile.Path);
			await DisplayAlert("App", shareResult ? "Published" : "Error", "Ok");
		}

		async void Handle_Clicked_Post_Message(object sender, System.EventArgs e)
		{
			var message = _txtMessage.Text;
			if (string.IsNullOrWhiteSpace(message))
			{
				await DisplayAlert("App", "Empty Message", "Ok");
				return;
			}
			var shareResult = await _manager.PostText(message);
			await DisplayAlert("App", shareResult ? "Published" : "Error", "Ok");
		}

		void Handle_ShowingLoggedOutUser(object sender, System.EventArgs e)
		{
			CleanUserData();
		}

		void Handle_ShowingLoggedInUser(object sender, XamarinFormsFacebook.FacebookLoginEventArgs e)
		{
			SetFieldsValues(e.User);
		}

		void CleanUserData()
		{
			_userImage.Source = "";
			_lblId.Text = "Id:";
			_lblToken.Text = "Token:";
			_lblFirstName.Text = "FirstName:";
			_lblLastName.Text = "LastName:";
			_lblEmail.Text = "Email:";
		}

		void SetFieldsValues(FacebookUser user)
		{
			_userImage.Source = user.Picture;
			_lblId.Text = $"Id: {user.Id}";
			_lblToken.Text = $"Token: {user.Token}";
			_lblFirstName.Text = $"FirstName: {user.FirstName}";
			_lblLastName.Text = $"LastName: {user.LastName}";
			_lblEmail.Text = $"Email: {user.Email}";
		}

		IFacebookManager _manager;

		public XamarinFormsFacebookPage()
		{
			_manager = DependencyService.Get<IFacebookManager>();
			if (_manager == null)
				throw new NotImplementedException("No IFacebookManager implementation found");
			InitializeComponent();
		}
	}
}
