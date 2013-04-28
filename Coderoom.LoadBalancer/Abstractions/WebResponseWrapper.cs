using System;
using System.IO;
using System.Net;
using Coderoom.LoadBalancer.Utilities;

namespace Coderoom.LoadBalancer.Abstractions
{
	public class WebResponseWrapper : IWebResponse
	{
		readonly HttpWebResponse _webResponse;

		public WebResponseWrapper(HttpWebResponse webResponse)
		{
			_webResponse = webResponse;
		}

		public Stream GetResponseStream()
		{
			return _webResponse.GetResponseStream();
		}

		public string GetStatusLine()
		{
			return "HTTP/{0}.{1} {2} {3}".Fmt(_webResponse.ProtocolVersion.Major, _webResponse.ProtocolVersion.Minor, (int)_webResponse.StatusCode, _webResponse.StatusDescription);
		}

		public void Dispose()
		{
			_webResponse.Dispose();
		}
	}

	public interface IWebResponse : IDisposable
	{
		Stream GetResponseStream();
		string GetStatusLine();
	}
}