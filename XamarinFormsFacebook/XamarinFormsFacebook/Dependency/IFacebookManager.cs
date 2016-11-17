using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsFacebook
{
	public interface IFacebookManager
	{
		Task<bool> SimpleLogin();
		Task<FacebookUser> Login();
		Task LogOut();
		Task<bool> ValidateToken();
		Task<bool> PostText(string message);
		Task<bool> PostPhoto(ImageSource image);
	}
}
