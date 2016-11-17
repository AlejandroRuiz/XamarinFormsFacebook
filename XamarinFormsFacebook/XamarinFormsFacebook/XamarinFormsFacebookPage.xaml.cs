using Xamarin.Forms;

namespace XamarinFormsFacebook
{
	public partial class XamarinFormsFacebookPage : ContentPage
	{
		void Handle_ShowingLoggedOutUser(object sender, System.EventArgs e)
		{
			_userImage.Source = "";
			_lblId.Text = "Id:";
			_lblToken.Text = "Token:";
			_lblFirstName.Text = "FirstName:";
			_lblLastName.Text = "LastName:";
			_lblEmail.Text = "Email:";
		}

		void Handle_ShowingLoggedInUser(object sender, XamarinFormsFacebook.FacebookLoginEventArgs e)
		{
			_userImage.Source = e.User.Picture;
			_lblId.Text = $"Id: {e.User.Id}";
			_lblToken.Text = $"Token: {e.User.Token}";
			_lblFirstName.Text = $"FirstName: {e.User.FirstName}";
			_lblLastName.Text = $"LastName: {e.User.LastName}";
			_lblEmail.Text = $"Email: {e.User.Email}";
		}

		public XamarinFormsFacebookPage()
		{
			InitializeComponent();
		}
	}
}
