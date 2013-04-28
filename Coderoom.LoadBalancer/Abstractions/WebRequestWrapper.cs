using System;
using System.Net;

namespace Coderoom.LoadBalancer.Abstractions
{
	public class WebRequestWrapper : IWebRequest
	{
		readonly HttpWebRequest _webRequest;

		public WebRequestWrapper(HttpWebRequest webRequest)
		{
			_webRequest = webRequest;
		}

		public void AddHeaders(WebHeaderCollection headers)
		{
			foreach (var headerKey in headers.AllKeys)
			{
				if (headerKey.ToLower().Equals("host"))
				{
					// host is already set, so ignore this header
				}
				else if (headerKey.ToLower().Equals("connection"))
				{
					// obsolete under HTTP/1.1
					// http://web.archive.org/web/20100813132504/http://www.io.com/~maus/HttpKeepAlive.html
				}
				else if (headerKey.ToLower().Equals("accept"))
				{
					_webRequest.Accept = headers[headerKey];
				}
				else if (headerKey.ToLower().Equals("user-agent"))
				{
					_webRequest.UserAgent = headers[headerKey];
				}
				else if (headerKey.ToLower().Equals("if-modified-since"))
				{
					_webRequest.IfModifiedSince = Convert.ToDateTime(headers[headerKey]);
				}
				else if (headerKey.ToLower().Equals("accept-encoding"))
				{
					// not supported, yet
				}
				else
				{
					_webRequest.Headers.Add(headerKey, headers[headerKey]);
				}
			}
		}

		public IWebResponse GetResponse()
		{
			WebResponse webResponse;

			try
			{
				webResponse = _webRequest.GetResponse();
			}
			catch (WebException wex)
			{
				webResponse = wex.Response;
			}

			return new WebResponseWrapper((HttpWebResponse)webResponse);
		}
	}

	public interface IWebRequest
	{
		void AddHeaders(WebHeaderCollection headers);
		IWebResponse GetResponse();
	}
}