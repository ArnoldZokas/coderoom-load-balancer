using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Coderoom.LoadBalancer.Request;
using Coderoom.LoadBalancer.Response;

namespace Coderoom.LoadBalancer
{
	public class HttpProxy
	{
		readonly IEnumerable<IPEndPoint> _servers;
		readonly IPortListener _listener;
		readonly IRequestMessageBuilder _requestMessageBuilder;
		readonly IResponseStreamWriter _responseStreamWriter;

		public HttpProxy(IEnumerable<IPEndPoint> servers, IPortListener listener, IRequestMessageBuilder requestMessageBuilder, IResponseStreamWriter responseStreamWriter)
		{
			_servers = servers;
			_listener = listener;
			_requestMessageBuilder = requestMessageBuilder;
			_responseStreamWriter = responseStreamWriter;
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
			// TODO: Abstract server selection
			var selectedServer = _servers.First();

			using (var clientStream = connectionEstablishedEventArgs.Client.GetStream())
			using (var client = HttpProxyConfiguration.HttpClientFactory())
			using (var requestMessage = _requestMessageBuilder.BuildRequestFromRequestStream(selectedServer, clientStream))
			{
				if (requestMessage == null)
				{
					var responseMessage = new HttpResponseMessage
						{
							StatusCode = HttpStatusCode.NotFound
						};
					_responseStreamWriter.WriteHttpResponseToClientStream(responseMessage, clientStream);
				}
				else
				{
					var responseMessage = client.SendAsync(requestMessage).Result;
					using (responseMessage)
						_responseStreamWriter.WriteHttpResponseToClientStream(responseMessage, clientStream);
				}
			}
		}
	}
}