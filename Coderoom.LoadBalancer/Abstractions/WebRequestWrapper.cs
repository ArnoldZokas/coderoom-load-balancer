using System;
using System.Collections.Generic;
using System.Net;

namespace Coderoom.LoadBalancer.Abstractions
{
	public class WebRequestWrapper : IWebRequest
	{
		readonly Dictionary<string, Action<HttpWebRequest, string>> _headerMap;
		readonly HttpWebRequest _webRequest;

		public WebRequestWrapper(HttpWebRequest webRequest)
		{
			_webRequest = webRequest;
			_headerMap = BuildHeaderMap();
		}

		public void AddHeaders(WebHeaderCollection headers)
		{
			foreach (var headerKey in headers.AllKeys)
			{
				var key = headerKey.ToLower();
				Action<HttpWebRequest, string> mapper;
				if (_headerMap.TryGetValue(key, out mapper))
				{
					mapper(_webRequest, headers[headerKey]);
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

		static Dictionary<string, Action<HttpWebRequest, string>> BuildHeaderMap()
		{
			var map = new Dictionary<string, Action<HttpWebRequest, string>>
				{
					{
						"host", (request, value) =>
							{
								// host is already set, so ignore this header
							}
					},
					{
						"connection", (request, value) =>
							{
								// obsolete under HTTP/1.1 (http://web.archive.org/web/20100813132504/http://www.io.com/~maus/HttpKeepAlive.html)
							}
					},
					{
						"accept-encoding", (request, value) =>
							{
								// not supported, yet
							}
					},
					{
						"accept", (request, value) => { request.Accept = value; }
					},
					{
						"user-agent", (request, value) => { request.UserAgent = value; }
					},
					{
						"if-modified-since", (request, value) => { request.IfModifiedSince = Convert.ToDateTime(value); }
					}
				};

			return map;
		}
	}

	public interface IWebRequest
	{
		void AddHeaders(WebHeaderCollection headers);
		IWebResponse GetResponse();
	}
}