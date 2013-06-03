using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Coderoom.LoadBalancer.Request;
using Coderoom.LoadBalancer.Utilities;

namespace Coderoom.LoadBalancer
{
	public class HttpProxy
	{
		readonly IEnumerable<IPEndPoint> _servers;
		readonly IPortListener _listener;
		readonly IRequestBuilder _requestBuilder;

		public HttpProxy(IEnumerable<IPEndPoint> servers, IPortListener listener, IRequestBuilder requestBuilder)
		{
			_servers = servers;
			_listener = listener;
			_requestBuilder = requestBuilder;
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
			// TODO: Abstract server selection
			var selectedServer = _servers.First();

			using (var clientStream = connectionEstablishedEventArgs.Client.GetStream())
			using (var client = HttpProxyConfiguration.HttpClientFactory())
			using (var requestMessage = _requestBuilder.BuildRequestFromRequestStream(selectedServer, clientStream))
			{
				var sendAsyncTask = client.SendAsync(requestMessage);
				sendAsyncTask.Start();
				sendAsyncTask.Wait();

				using (var responseMessage = sendAsyncTask.Result)
				{
					var responseBuilder = new StringBuilder();
					responseBuilder.AppendLine("HTTP/{0}.{1} {2} {3}".Fmt(responseMessage.Version.Major, responseMessage.Version.Minor, (int)responseMessage.StatusCode, responseMessage.ReasonPhrase));
					using (var swriter = new StreamWriter(clientStream))
					{
						var responseHeaders = responseMessage.Headers;
						foreach (var header in responseHeaders)
						{
							responseBuilder.AppendLine(string.Format("{0}: {1}", header.Key, header.Value.First()));
						}

						var content = responseMessage.Content.ReadAsStringAsync().Result;
						if (content != string.Empty)
						{
							responseBuilder.AppendLine("\n");
							responseBuilder.Append(content);
						}

						var response = responseBuilder.ToString().TrimEnd('\n');
						swriter.Write(response);
					}
				}
			}
			OnRequestProcessed(EventArgs.Empty);
		}
	}
}