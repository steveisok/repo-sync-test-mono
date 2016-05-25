//
// System.Net.WebProxy.cs
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Mono.Net;

namespace System.Net 
{
	[Serializable]
	public class WebProxy : IWebProxy, ISerializable
	{
		Uri address;
		bool bypassOnLocal;
		ArrayList bypassList;
		ICredentials credentials;
		bool useDefaultCredentials;

		// Constructors

		public WebProxy ()
			: this ((Uri) null, false, null, null) {}

		public WebProxy (string address)
			: this (ToUri (address), false, null, null) {}

		public WebProxy (Uri address) 
			: this (address, false, null, null) {}

		public WebProxy (string address, bool bypassOnLocal) 
			: this (ToUri (address), bypassOnLocal, null, null) {}

		public WebProxy (string host, int port)
			: this (new Uri ("http://" + host + ":" + port)) {}

		public WebProxy (Uri address, bool bypassOnLocal)
			: this (address, bypassOnLocal, null, null) {}

		public WebProxy (string address, bool bypassOnLocal, string [] bypassList)
			: this (ToUri (address), bypassOnLocal, bypassList, null) {}

		public WebProxy (Uri address, bool bypassOnLocal, string [] bypassList)
			: this (address, bypassOnLocal, bypassList, null) {}

		public WebProxy (string address, bool bypassOnLocal, string [] bypassList,
				ICredentials credentials)
			: this (ToUri (address), bypassOnLocal, bypassList, credentials) {}

		public WebProxy (Uri address, bool bypassOnLocal, 
				 string[] bypassList, ICredentials credentials)
		{
			this.address = address;
			this.bypassOnLocal = bypassOnLocal;
			if (bypassList != null)
				this.bypassList = new ArrayList (bypassList);
			this.credentials = credentials;
			CheckBypassList ();
		}

		protected WebProxy (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			this.address = (Uri) serializationInfo.GetValue ("_ProxyAddress", typeof (Uri));
			this.bypassOnLocal = serializationInfo.GetBoolean ("_BypassOnLocal");
			this.bypassList = (ArrayList) serializationInfo.GetValue ("_BypassList", typeof (ArrayList));
			this.useDefaultCredentials =  serializationInfo.GetBoolean ("_UseDefaultCredentials");
			this.credentials = null;
			CheckBypassList ();
		}

		// Properties
		public Uri Address {
			get { return address; }
			set { address = value; }
		}

		public ArrayList BypassArrayList {
			get {
				if (bypassList == null)
					bypassList = new ArrayList ();
				return bypassList;
			}
		}

		public string [] BypassList {
			get { return (string []) BypassArrayList.ToArray (typeof (string)); }
			set { 
				if (value == null)
					throw new ArgumentNullException ();
				bypassList = new ArrayList (value); 
				CheckBypassList ();
			}
		}

		public bool BypassProxyOnLocal {
			get { return bypassOnLocal; }
			set { bypassOnLocal = value; }
		}

		public ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
		}

		[MonoTODO ("Does not affect Credentials, since CredentialCache.DefaultCredentials is not implemented.")]
		public bool UseDefaultCredentials {
			get { return useDefaultCredentials; }
			set { useDefaultCredentials = value; }
		}

		// Methods
		[Obsolete ("This method has been deprecated", false)]
		[MonoTODO("Can we get this info under windows from the system?")]
		public static WebProxy GetDefaultProxy ()
		{
			// Select gets a WebProxy from config files, if available.
			IWebProxy p = GlobalProxySelection.Select;
			if (p is WebProxy)
				return (WebProxy) p;

			return new WebProxy ();
		}

		public Uri GetProxy (Uri destination)
		{
			if (IsBypassed (destination))
				return destination;

			return address;
		}

		public bool IsBypassed (Uri host)
		{
			if (host == null)
				throw new ArgumentNullException ("host");

			if (host.IsLoopback && bypassOnLocal)
				return true;

			if (address == null)
				return true;

			string server = host.Host;
			if (bypassOnLocal && server.IndexOf ('.') == -1)
				return true;

			// LAMESPEC
			if (!bypassOnLocal) {
				if (String.Compare (server, "localhost", true, CultureInfo.InvariantCulture) == 0)
					return true;
				if (String.Compare (server, "loopback", true, CultureInfo.InvariantCulture) == 0)
					return true;

				IPAddress addr = null;
				if (IPAddress.TryParse (server, out addr) && IPAddress.IsLoopback (addr))
					return true;
			}

			if (bypassList == null || bypassList.Count == 0)
				return false;

			try {
				string hostStr = host.Scheme + "://" + host.Authority;
				int i = 0;
				for (; i < bypassList.Count; i++) {
					Regex regex = new Regex ((string) bypassList [i], 
						// TODO: RegexOptions.Compiled |  // not implemented yet by Regex
						RegexOptions.IgnoreCase |
						RegexOptions.Singleline);

					if (regex.IsMatch (hostStr))
						break;
				}

				if (i == bypassList.Count)
					return false;

				// continue checking correctness of regular expressions..
				// will throw expression when an invalid one is found
				for (; i < bypassList.Count; i++)
					new Regex ((string) bypassList [i]);

				return true;
			} catch (ArgumentException) {
				return false;
			}
		}

		protected virtual void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			serializationInfo.AddValue ("_BypassOnLocal", bypassOnLocal);
			serializationInfo.AddValue ("_ProxyAddress", address);
			serializationInfo.AddValue ("_BypassList", bypassList);
			serializationInfo.AddValue ("_UseDefaultCredentials", UseDefaultCredentials);
		}

		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		                                  StreamingContext streamingContext)
		{
			GetObjectData (serializationInfo, streamingContext);
		}

		// Private Methods
		// this compiles the regular expressions, and will throw
		// an exception when an invalid one is found.
		void CheckBypassList ()
		{
			if (bypassList == null)
				return;
			for (int i = 0; i < bypassList.Count; i++)
				new Regex ((string) bypassList [i]);
		}

		static Uri ToUri (string address)
		{
			if (address == null)
				return null;
				
			if (address.IndexOf ("://", StringComparison.Ordinal) == -1) 
				address = "http://" + address;

			return new Uri (address);
		}

		internal static IWebProxy CreateDefaultProxy ()
		{
#if MONOTOUCH
			return CFNetwork.GetDefaultProxy ();
#elif MONODROID
			// Return the system web proxy.  This only works for ICS+.
			var androidProxy = AndroidPlatform.GetDefaultProxy ();
			if (androidProxy != null)
				return androidProxy;

			return new WebProxy (false);
#else
			if (Platform.IsMacOS)
				return CFNetwork.GetDefaultProxy ();

			return new WebProxy (false);
#endif
		}

		// Global settings initialization
		private WebProxy (bool enableAutoproxy)
		{
#if !MOBILE			
			if (IsWindows () && InitializeRegistryGlobalProxy ())
				return;
#endif
			string address = Environment.GetEnvironmentVariable ("http_proxy") ?? Environment.GetEnvironmentVariable ("HTTP_PROXY");

			if (address != null) {
				try {
					if (!address.StartsWith ("http://"))
						address = "http://" + address;

					Uri uri = new Uri (address);
					IPAddress ip;
					
					if (IPAddress.TryParse (uri.Host, out ip)) {
						if (IPAddress.Any.Equals (ip)) {
							UriBuilder builder = new UriBuilder (uri);
							builder.Host = "127.0.0.1";
							uri = builder.Uri;
						} else if (IPAddress.IPv6Any.Equals (ip)) {
							UriBuilder builder = new UriBuilder (uri);
							builder.Host = "[::1]";
							uri = builder.Uri;
						}
					}

					bool bBypassOnLocal = false;
					ArrayList al = new ArrayList ();
					string bypass = Environment.GetEnvironmentVariable ("no_proxy") ?? Environment.GetEnvironmentVariable ("NO_PROXY");
					
					if (bypass != null) {
						string[] bypassList = bypass.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					
						foreach (string str in bypassList) {
							if (str != "*.local")
								al.Add (str);
							else
								bBypassOnLocal = true;
						}
					}

					this.address = uri;
					this.bypassOnLocal = bBypassOnLocal;
					this.bypassList = CreateBypassList (al);
					return;
				} catch (UriFormatException) {
				}
			}
		}

#if !MOBILE
		static bool IsWindows ()
		{
			return (int) Environment.OSVersion.Platform < 4;
		}
				
		bool InitializeRegistryGlobalProxy ()
		{
			int iProxyEnable = (int)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyEnable", 0);

			if (iProxyEnable > 0) {
				string strHttpProxy = "";
				bool bBypassOnLocal = false;
				ArrayList al = new ArrayList ();
				
				string strProxyServer = (string)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyServer", null);
				string strProxyOverrride = (string)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyOverride", null);
				
				if (strProxyServer.Contains ("=")) {
					foreach (string strEntry in strProxyServer.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
						if (strEntry.StartsWith ("http=")) {
							strHttpProxy = strEntry.Substring (5);
							break;
						}
				} else strHttpProxy = strProxyServer;
				
				if (strProxyOverrride != null) {
					string[] bypassList = strProxyOverrride.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				
					foreach (string str in bypassList) {
						if (str != "<local>")
							al.Add (str);
						else
							bBypassOnLocal = true;
					}
				}

				this.address = ToUri (strHttpProxy);
				this.bypassOnLocal = bBypassOnLocal;
				this.bypassList = CreateBypassList (al);
				return true;
			}

			return false;
		}
#endif

		// Takes an ArrayList of fileglob-formatted strings and returns an array of Regex-formatted strings
		static ArrayList CreateBypassList (ArrayList al)
		{
			string[] result = al.ToArray (typeof (string)) as string[];
			for (int c = 0; c < result.Length; c++)
			{
				result [c] = "^" +
					Regex.Escape (result [c]).Replace (@"\*", ".*").Replace (@"\?", ".") +
					"$";
			}
			return new ArrayList (result);
		}
	}
}
