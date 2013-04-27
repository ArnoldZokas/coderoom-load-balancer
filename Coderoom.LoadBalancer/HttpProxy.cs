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
		private readonly IPortListener _listener;
		private readonly IEnumerable<IPEndPoint> _servers;

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

		private void ListenerOnConnectionEstablished(object sender, ConnectionEstablishedEventArgs connectionEstablishedEventArgs)
		{
			IPEndPoint selectedServer = _servers.First();

			using (var clientStream = connectionEstablishedEventArgs.Client.GetStream())
			{
				string path;
				using (var clientSstreamReader = new StreamReader(clientStream))
				{
					string header = clientSstreamReader.ReadLine();
					path = header.Split(' ')[1];
				}

				var requestUri = new Uri(string.Format("http://{0}:{1}{2}", selectedServer.Address, selectedServer.Port, path));
				WebRequest proxiedRequest = WebRequest.Create(requestUri);

				using (WebResponse proxiedResponse = proxiedRequest.GetResponse())
				{
					using (Stream proxiedResponseStream = proxiedResponse.GetResponseStream())
					{
						using (var proxiedResponseStreamReader = new StreamReader(proxiedResponseStream))
						{
							string proxiedResponseBody = proxiedResponseStreamReader.ReadToEnd();

							var responseBuilder = new StringBuilder();
							responseBuilder.AppendLine("HTTP/1.1 200 OK");
							using (var swriter = new StreamWriter(clientStream))
							{
								foreach (var headerKey in proxiedResponse.Headers.AllKeys)
								{
									responseBuilder.AppendLine(string.Format("{0}: {1}", headerKey, proxiedResponse.Headers[headerKey]));
								}

								responseBuilder.AppendLine();
								responseBuilder.Append(proxiedResponseBody);

								string response = responseBuilder.ToString();
								swriter.Write(response);
								swriter.Flush();
							}
						}
					}
				}
			}
		}
	}
}