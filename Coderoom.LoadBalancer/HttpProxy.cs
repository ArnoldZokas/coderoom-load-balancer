using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Coderoom.LoadBalancer
{
	public class HttpProxy
	{
		readonly IPortListener _listener;
		readonly IEnumerable<IPEndPoint> _servers;

		public HttpProxy(IEnumerable<IPEndPoint> servers, IPortListener listener)
		{
			_listener = listener;
			_servers = servers;
		}

		public void Start()
		{
			_listener.ConnectionEstablished += ListenerOnConnectionEstablished;
			_listener.Start();
		}

		public void Stop()
		{
			_listener.ConnectionEstablished -= ListenerOnConnectionEstablished;
			_listener.Stop();
		}

		void ListenerOnConnectionEstablished(object sender, ConnectionEstablishedEventArgs connectionEstablishedEventArgs)
		{
			var selectedServer = _servers.First();

			using (var clientStream = connectionEstablishedEventArgs.Client.GetStream())
			{
				var requestParserResult = HttpProxyConfiguration.RawHttpRequestParser.ParseHttpRequestFromClientStream(clientStream);
				var requestUri = BuildRequestUri(selectedServer, requestParserResult.RequestUri);
				var proxiedRequest = HttpProxyConfiguration.WebRequestFactory(requestUri);
				proxiedRequest.AddHeaders(requestParserResult.RequestHeaders);

				using (var proxiedResponse = proxiedRequest.GetResponse())
				using (var proxiedResponseStream = proxiedResponse.GetResponseStream())
				using (var proxiedResponseStreamReader = HttpProxyConfiguration.StreamReaderFactory(proxiedResponseStream, false))
				{
					var proxiedResponseBody = proxiedResponseStreamReader.ReadToEnd();

					var responseBuilder = new StringBuilder();
					responseBuilder.AppendLine(proxiedResponse.GetStatusLine());
					using (var swriter = new StreamWriter(clientStream))
					{
						var responseHeaders = proxiedResponse.GetHeaders();
						foreach (var headerKey in responseHeaders.AllKeys)
						{
							responseBuilder.AppendLine(string.Format("{0}: {1}", headerKey, responseHeaders[headerKey]));
						}

						responseBuilder.AppendLine();
						responseBuilder.Append(proxiedResponseBody);

						var response = responseBuilder.ToString();
						swriter.Write(response);
					}
				}
			}
		}

		static Uri BuildRequestUri(IPEndPoint endpoint, string relativeRequestUri)
		{
			return new Uri(string.Format("http://{0}:{1}{2}", endpoint.Address, endpoint.Port, relativeRequestUri));
		}
	}
}