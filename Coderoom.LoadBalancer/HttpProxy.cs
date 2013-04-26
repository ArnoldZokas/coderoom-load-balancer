using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Coderoom.LoadBalancer
{
	public class HttpProxy
	{
		private readonly TcpListener _listener;
		private readonly Thread _listenerThread;
		private readonly IEnumerable<IPEndPoint> _servers;

		public HttpProxy(IPEndPoint target, IEnumerable<IPEndPoint> servers)
		{
			_listener = new TcpListener(target);
			_servers = servers;
			_listenerThread = new Thread(Listen);
		}

		public void Start()
		{
			_listenerThread.Start(_listener);
		}

		private void Listen(object obj)
		{
			var listener = (TcpListener)obj;
			listener.Start();

			while (true)
			{
				var tcpClient = listener.AcceptTcpClient();
				
				ProcessRequest(tcpClient);
				
				tcpClient.Close();
			}
		}

		private void ProcessRequest(TcpClient client)
		{
			var selectedServer = _servers.First();

			using (var stream = client.GetStream())
			{
				using (var streamReader = new StreamReader(stream))
				{
					var header = streamReader.ReadLine();
					var path = header.Split(' ')[1];

					var requestUri = new Uri(string.Format("http://{0}:{1}{2}", selectedServer.Address, selectedServer.Port, path));
					var proxiedRequest = WebRequest.Create(requestUri);

					using (var proxiedResponse = proxiedRequest.GetResponse())
					{
						using (var proxiedResponseStream = proxiedResponse.GetResponseStream())
						{
							using (var proxiedResponseStreamReader = new StreamReader(proxiedResponseStream))
							{
								var proxiedResponseBody = proxiedResponseStreamReader.ReadToEnd();

								var responseBuilder = new StringBuilder();
								responseBuilder.AppendLine("HTTP/1.1 200 OK");
								using (var swriter = new StreamWriter(stream))
								{
									foreach (var headerKey in proxiedResponse.Headers.AllKeys)
									{
										responseBuilder.AppendLine(string.Format("{0}: {1}", headerKey, proxiedResponse.Headers[headerKey]));
									}

									responseBuilder.AppendLine();
									responseBuilder.Append(proxiedResponseBody);

									var response = responseBuilder.ToString();
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
}