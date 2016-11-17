using System;
namespace XamarinFormsFacebook
{
	public static class Settings
	{
		public static string[] ReadPermissions = new string[] {
			"public_profile", "email"
		};

		public static string[] PublishPermissions = new string[] {
			"publish_actions"
		};

		public static string AppName { get; } = "XamarinGDLGroup";
	}
}
