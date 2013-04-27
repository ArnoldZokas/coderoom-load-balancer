using System;
using System.IO;
using System.Net;

namespace Coderoom.LoadBalancer.Abstractions
{
	public class WebResponseWrapper : IWebResponse
	{
		readonly WebResponse _webResponse;

		public WebResponseWrapper(WebResponse webResponse)
		{
			_webResponse = webResponse;
		}

		public Stream GetResponseStream()
		{
			return _webResponse.GetResponseStream();
		}

		public void Dispose()
		{
			_webResponse.Dispose();
		}
	}

	public interface IWebResponse : IDisposable
	{
		Stream GetResponseStream();
	}
}