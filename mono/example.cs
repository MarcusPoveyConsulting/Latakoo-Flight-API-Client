using System;
using Newtonsoft.Json.Linq;

namespace Latakoo
{
	class MainClass
	{
		public static string email = "My email";
		public static string password = "My password";
		
		public static void Main (string[] args)
		{
			Latakoo koo = new Latakoo();
			
			// List API
			JObject response = koo.systemApiList();
			Console.WriteLine(response.ToString());
			
			// Authenticate a user
			try {
				JObject response2 = koo.userAuthenticate(email, password) ;
				Console.WriteLine("User has been authenticated, here are their details...");
				Console.WriteLine(response2.ToString());
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
			
			// Get my details
			try {
				int userid = koo.getAuthenticatedUserID();
				JObject response3 = koo.userGetById(userid);
				Console.WriteLine("That's you that is...");
				Console.WriteLine(response3.ToString());
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
			
		}
	}
}
