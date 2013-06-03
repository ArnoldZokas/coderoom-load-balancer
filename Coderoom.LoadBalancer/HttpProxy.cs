using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Coderoom.LoadBalancer.Utilities;

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

		public event EventHandler<EventArgs> RequestProcessed;

		protected virtual void OnRequestProcessed(EventArgs e)
		{
			if (RequestProcessed != null)
				RequestProcessed(this, e);
		}

		async void ListenerOnConnectionEstablished(object sender, ConnectionEstablishedEventArgs connectionEstablishedEventArgs)
		{
			var selectedServer = _servers.First();

			using (var clientStream = connectionEstablishedEventArgs.Client.GetStream())
			{
				var requestParserResult = HttpProxyConfiguration.RawHttpRequestParser.ParseHttpRequestFromClientStream(clientStream);
				var requestUri = BuildRequestUri(selectedServer, requestParserResult.RequestUri);

				using (var client = HttpProxyConfiguration.HttpClientFactory())
				{
					var httpRequestMessage = new HttpRequestMessage();
					httpRequestMessage.RequestUri = requestUri;
					httpRequestMessage.Content = new StringContent(string.Empty);
					var h = requestParserResult.RequestHeaders;
					foreach (var name in h.AllKeys)
					{
						httpRequestMessage.Headers.Add(name, h.Get(name));
					}
					
					//var x = await client.SendAsync(httpRequestMessage);
					var task = client.SendAsync(httpRequestMessage);
					task.Start();
					task.Wait();
					var x = task.Result;
					httpRequestMessage.Dispose();

					var responseBuilder = new StringBuilder();
					responseBuilder.AppendLine("HTTP/{0}.{1} {2} {3}".Fmt(x.Version.Major, x.Version.Minor, (int)x.StatusCode, x.ReasonPhrase));
					using (var swriter = new StreamWriter(clientStream))
					{
						var responseHeaders = x.Headers;
						foreach (var header in responseHeaders)
						{
							responseBuilder.AppendLine(string.Format("{0}: {1}", header.Key, header.Value.First()));
						}

						var content = x.Content.ReadAsStringAsync().Result;
						if (content != string.Empty)
						{
							responseBuilder.AppendLine("\n");
							responseBuilder.Append(content);
						}
						
						var response = responseBuilder.ToString().TrimEnd('\n');
						swriter.Write(response);
					}
					x.Dispose();
				}
			}
			OnRequestProcessed(EventArgs.Empty);
		}

		static Uri BuildRequestUri(IPEndPoint endpoint, string relativeRequestUri)
		{
			return new Uri(string.Format("http://{0}:{1}{2}", endpoint.Address, endpoint.Port, relativeRequestUri));
		}
	}
}