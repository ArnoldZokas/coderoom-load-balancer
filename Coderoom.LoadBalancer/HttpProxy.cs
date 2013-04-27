using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Coderoom.LoadBalancer.Abstractions;

namespace Coderoom.LoadBalancer
{
	public class HttpProxy
	{
		readonly IPortListener _listener;
		readonly IEnumerable<IPEndPoint> _servers;
		readonly Func<Stream, bool, TextReader> _streamReaderFactory;
		readonly Func<Uri, IWebRequest> _webRequestFactory;

		public HttpProxy(IEnumerable<IPEndPoint> servers, IPortListener listener, Func<Stream, bool, TextReader> streamReaderFactory, Func<Uri, IWebRequest> webRequestFactory)
		{
			_listener = listener;
			_streamReaderFactory = streamReaderFactory;
			_webRequestFactory = webRequestFactory;
			_servers = servers;
		}

		public void Start()
		{
			_listener.ConnectionEstablished += ListenerOnConnectionEstablished;
			_listener.Start();
		}

		void ListenerOnConnectionEstablished(object sender, ConnectionEstablishedEventArgs connectionEstablishedEventArgs)
		{
			var selectedServer = _servers.First();

			using (var clientStream = connectionEstablishedEventArgs.Client.GetStream())
			{
				var relativeRequestUri = GetRequestUri(clientStream);
				var requestUri = BuildRequestUri(selectedServer, relativeRequestUri);
				var proxiedRequest = _webRequestFactory(requestUri);

				using (var proxiedResponse = proxiedRequest.GetResponse())
				using (var proxiedResponseStream = proxiedResponse.GetResponseStream())
				using (var proxiedResponseStreamReader = _streamReaderFactory(proxiedResponseStream, false))
				{
					var proxiedResponseBody = proxiedResponseStreamReader.ReadToEnd();

					var responseBuilder = new StringBuilder();
					//responseBuilder.AppendLine("HTTP/1.1 200 OK");
					using (var swriter = new StreamWriter(clientStream))
					{
						//foreach (var headerKey in proxiedResponse.Headers.AllKeys)
						//{
						//	responseBuilder.AppendLine(string.Format("{0}: {1}", headerKey, proxiedResponse.Headers[headerKey]));
						//}

						responseBuilder.AppendLine();
						responseBuilder.Append(proxiedResponseBody);

						var response = responseBuilder.ToString();
						swriter.Write(response);
					}
				}
			}
		}

		string GetRequestUri(Stream clientStream)
		{
			/* Request line format per HTTP specification http://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html#sec5.1
			 * 
			 *		REQUEST-LINE = Method Request-URI HTTP-Version CRLF
			 * 
			 */

			using (var clientSstreamReader = _streamReaderFactory(clientStream, true))
			{
				var requestLine = clientSstreamReader.ReadLine();
				var requestLineFragments = requestLine.Split(' ');

				const int pathFragmentPosition = 1;
				return requestLineFragments[pathFragmentPosition];
			}
		}

		static Uri BuildRequestUri(IPEndPoint endpoint, string relativeRequestUri)
		{
			return new Uri(string.Format("http://{0}:{1}{2}", endpoint.Address, endpoint.Port, relativeRequestUri));
		}
	}
}