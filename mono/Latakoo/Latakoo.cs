/**
 * Latakoo Flight API Library (.NET 3.5 / Mono)
 * 
 * @author Marcus Povey <marcus@marcus-povey.co.uk>
 * @copyright Marcus Povey 2012
 * @link http://www.marcus-povey.co.uk
 * @link https://github.com/mapkyca/Latakoo-Flight-API-Client
 * @link http://latakoo.com
 * 
 * Copyright (c) 2011-12 Marcus Povey
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Newtonsoft.Json; // Requires Json.net http://james.newtonking.com
using Newtonsoft.Json.Linq;


namespace Latakoo
{
	
	
	public class Latakoo
	{
		private string email;
		private string password;
		private string token;
		private int userid;
		private string endpoint;
		
		public Latakoo ()
		{
			this.email = "";
			this.password = "";
			this.token = "";
			this.userid = 0;
			this.endpoint = "latakoo.com";
		}
		
		public Latakoo(String endpoint)
		{
			this.email = "";
			this.password = "";
			this.token = "";
			this.userid = 0;
			this.endpoint = endpoint;
		}
		
		public string getEndpoint() { return this.endpoint; }
		public int getAuthenticatedUserID() { return this.userid; } 
		public string getAuthenticatedUserToken() { return this.token; }
		
		/**
		 * Convoluted way to get unix timestamp.
		 */
		protected int Time()
		{
			DateTime now = DateTime.Now;
			
			TimeSpan span = (now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
			
			return (int)span.TotalSeconds;
		}
		
		/**
		 * .Net version of PHP's md5 function.
		 */
		protected string md5(string input)
		{
			MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
    		byte[] hash = md5.ComputeHash(inputBytes);
			
			StringBuilder sb = new StringBuilder();
		    for (int i = 0; i < hash.Length; i++)
		    {
		        sb.Append(hash[i].ToString("x2"));
		    }
			return sb.ToString();
		}
		
		/**
		 * Sign a query with a given token.
		 */
		protected String sign(string query, string email, string password, string token)
		{
			query = query.Trim();
			email = email.Trim();
			password = password.Trim();
			token = token.Trim();
			
			int now = this.Time(); 
			
			return email + ":" + now + ":" + this.md5(this.md5(password + token) + query +now);
		}
		
		public JObject execute(string method)
		{
			return this.execute(method, null);
		}
		
		public JObject execute(string method, OrderedDictionary parameters)
		{
			return this.execute(method, parameters, null);
		}
		
		public JObject execute(string method, OrderedDictionary parameters, OrderedDictionary call_details)
		{
			return this.execute(method, parameters, call_details, this.endpoint);
		}
		
		/**
		 * Execute an API call against the gateway.
		 * 
		 * @param string method Method being called
		 * @param OrderedDictionary parameters Associated list of parameters in API call order.
		 * @param OrderedDictionary call_details Additional call details, including:
		 *						'method' => 'GET' | 'POST'
		 * 						'headers' => Array of HTTP headers to send.
		 * 						'postdata' => If 'method' == POST then this is the data to send. 
		 * 						'token' => User authenticating token as retrieved by a call to
		 * 									Latakoo::authenticate('email','password');
		 * 						'email' => Email address of the signing user used in 'token'
		 * 						'password' => Password of the signing user used in 'token'
		 * 		
		 * @param string endpoint The Endpoint to direct the query.
		 */
		public JObject execute(string method, OrderedDictionary parameters, OrderedDictionary call_details, string endpoint)
		{
			if (endpoint == null) endpoint = this.endpoint;
			if (method == null) return null;
			
			if (call_details == null) call_details = new OrderedDictionary();
			if (parameters == null) parameters = new OrderedDictionary();
			
			if (!call_details.Contains("method")) call_details.Add("method", "GET");
			if (!call_details.Contains("headers")) call_details.Add("headers", new OrderedDictionary());
			
			// Construct query string
			List<string> queryparams = new List<string>();
			queryparams.Add("method=" + HttpUtility.UrlEncode(method));
			
			foreach (DictionaryEntry de in parameters)
			{
				if (de.Value is OrderedDictionary)
				{
					OrderedDictionary val = (OrderedDictionary)de.Value;
					foreach (DictionaryEntry de2 in val)
						queryparams.Add(HttpUtility.UrlEncode(de.Key.ToString()) + "[" + HttpUtility.UrlEncode(de2.Key.ToString()) + "]=" + HttpUtility.UrlEncode(de2.Value.ToString()));
				}
				else
					queryparams.Add(HttpUtility.UrlEncode(de.Key.ToString()) + "=" + HttpUtility.UrlEncode(de.Value.ToString()));
			}
			
			string[] queryparamsarray = queryparams.ToArray();
			string query = string.Join("&", queryparamsarray);
			
			endpoint = "https://" + endpoint + "/-/api/?" + query;
		
			// Authenticate command
			if (call_details.Contains("email") && call_details.Contains("password") && call_details.Contains("token"))
			{
				string token = this.sign(query, (string)call_details["email"], (string)call_details["password"], (string)call_details["token"]);
				
				OrderedDictionary headers = (OrderedDictionary)call_details["headers"];
				headers.Add("X_PFTP_API_TOKEN", token);
				call_details.Remove("headers");
				call_details.Add("headers", headers);
			}
			
			// Make request
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(endpoint);
			request.UserAgent = "Mono Bindings v1";
			request.Method = (string)call_details["method"];
			request.Credentials = CredentialCache.DefaultCredentials;
			
			OrderedDictionary httpheaders = (OrderedDictionary)call_details["headers"];
			WebHeaderCollection webheaders = new WebHeaderCollection();
			foreach (DictionaryEntry de in httpheaders)
			{
				webheaders.Add(de.Key + ": " + de.Value);
			}
			request.Headers = webheaders;
			
			ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate);

						
			if (call_details.Contains("postdata"))
			{
				// Send post data
				byte[] postBytes = System.Text.Encoding.ASCII.GetBytes((string)call_details["postdata"]);
				request.ContentLength = postBytes.Length;
				
				System.IO.Stream str = request.GetRequestStream();

        		str.Write(postBytes, 0, postBytes.Length);

        		str.Close();
			}
			
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			
			
			Stream stream = response.GetResponseStream();
			string raw_json = new StreamReader(stream).ReadToEnd();
			
			return JObject.Parse(raw_json);
		}
		
		public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			// TODO: Correctly validate server certificates. For now, accept any old nonsense.
			
			return true; 
		}
		
		/**
		 * List the API and parameters provided by the endpoint.
		 */
		public JObject systemApiList() 
		{
			return this.execute("system.api.list");
		}
		
		/**
		 * Authenticate a user against the latakoo Flight API.
		 * All further commands will be executed as the authenticated user.
		 */
		public JObject userAuthenticate(string email, string password)
		{
			email = email.Trim();
			password = password.Trim();
			
			OrderedDictionary parameters = new OrderedDictionary();
			parameters.Add("email", email);
			parameters.Add("password", password);
			
			JObject result = this.execute("user.authenticate.x", parameters);
			
			JToken status = result["status_code"];
			JToken message = result["message"];
			JToken resultblob = result["result"];
			
			int status_code = int.Parse(status.ToString());
			if (status_code == 0)
			{
				JToken authcode = resultblob["authcode"];
				string authcodesz = authcode.ToString();
				
				string [] token = authcodesz.Split(':');
				
				this.token = token[0];
				this.userid = int.Parse(token[1]);
				this.email = email;
				this.password = password;
				
				return result;
			}
			else
			{
				throw new Exception(message.ToString());
			}
			
		}
		
		/**
		 * Return the details of a specific user.
		 */
		public JObject userGetById(int id)
		{
			OrderedDictionary parameters = new OrderedDictionary();
			parameters.Add("id", id.ToString());
			
			OrderedDictionary call_details = new OrderedDictionary();
			call_details.Add("email", this.email);
			call_details.Add("password", this.password);
			call_details.Add("token", this.token);
			
			JObject result = this.execute("user.get.byid", parameters, call_details);
			
			JToken status = result["status_code"];
			JToken message = result["message"];
			JToken resultblob = result["result"];
			
			int status_code = int.Parse(status.ToString());
			if (status_code == 0)
			{
				return result;
			}
			else
			{
				throw new Exception(message.ToString());
			}
			
		}
		
	}
	
}
