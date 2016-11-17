using System;
namespace XamarinFormsFacebook
{
	public class FacebookUser
	{
		public FacebookUser(string id, string token, string firstName, string lastName, string email, string imageUrl)
		{
			Id = id;
			Token = token;
			FirstName = firstName;
			LastName = lastName;
			Email = email;
			Picture = imageUrl;
		}

		public string Id { get; set; }

		public string Token { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public string Picture { get; set; }
	}
}
