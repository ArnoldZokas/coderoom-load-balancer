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
	internal class Application
	{
		public static void Main()
		{
			var servers = new List<IPEndPoint>
				{
					new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081),
					new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8082)
				};

			var lb = new HttpLoadBalancerProxy(81, servers);
			lb.Start();
		}
	}

	public class HttpLoadBalancerProxy
	{
		private readonly IPAddress _ipAddress = new IPAddress(new byte[] {127, 0, 0, 1});
		private readonly TcpListener _listener;
		private readonly IEnumerable<IPEndPoint> _servers;
		private readonly Thread _listenerThread;

		public HttpLoadBalancerProxy(int port, IEnumerable<IPEndPoint> servers)
		{
			_servers = servers;
			_listener = new TcpListener(_ipAddress, port);
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
				var client = listener.AcceptTcpClient();
				ProcessRequest2(client);
				client.Close();
			}
		}

		private void ProcessRequest2(TcpClient client)
		{
			using (var stream = client.GetStream())
			{
				using (var reader = new StreamReader(stream))
				{
					IPEndPoint ipEndPoint = _servers.First();
					var requestUri = new Uri(string.Format("http://{0}:{1}/", ipEndPoint.Address, ipEndPoint.Port));
					var request = WebRequest.Create(requestUri);
					
					//using (var rs = request.GetRequestStream())
					//{
					//	var origHeaders = ReadAll(reader);
					//	using (var rsw = new StreamWriter(rs))
					//	{
					//		rsw.Write(origHeaders);
					//	}
					//}

					using (var r = request.GetResponse())
					{
						using (var rs = r.GetResponseStream())
						{
							using (var rsr = new StreamReader(rs))
							{
								var response = rsr.ReadToEnd();

								var x = new StringBuilder();
								x.AppendLine("HTTP/1.1 200 OK");
								using (var swriter = new StreamWriter(stream))
								{
									foreach (var headerKey in r.Headers.AllKeys)
									{
										x.AppendLine(string.Format("{0}: {1}", headerKey, r.Headers[headerKey]));
									}

									x.AppendLine();
									x.AppendLine(response);

									var remoteResponse = x.ToString();
									swriter.Write(remoteResponse);
									swriter.Flush();
								}
							}
						}
					}
				}
			}
		}

		private void ProcessRequest(TcpClient client)
		{
			using (var stream = client.GetStream())
			{
				using (var reader = new StreamReader(stream))
				{
					//var rawHeader = reader.ReadLine();
					//var headerFragments = rawHeader.Split(' ');
					//var httpMethod = headerFragments[0];
					//var relativeRequestPath = headerFragments[1];

					Console.WriteLine("== Proxying request to: " + _servers.Skip(1).First());
					var proxyClient = new TcpClient();
					proxyClient.Connect(_servers.First());
					
					using (var proxyClientStream = proxyClient.GetStream())
					{
						var sw = new System.IO.StreamWriter(proxyClientStream);
						var sr = new System.IO.StreamReader(proxyClientStream);

						var rs = ReadAll(reader);
						sw.WriteLine(rs);
						sw.Flush();

						
						string remoteResponse = ReadAll2(sr);
						Console.WriteLine(remoteResponse);

						//remoteResponse = "HTTP/1.1 200 OK\r\n" +
						//				 "Content-Type: text/html\r\n" +
						//				 "Last-Modified: Fri, 26 Apr 2013 19:21:07 GMT\r\n" +
						//				 "Accept-Ranges: bytesETag: \"80ae8633b342ce1:0\"\r\n" +
						//				 "Vary: Accept-Encoding\r\n" +
						//				 "Server: Microsoft-IIS/8.0\r\n" +
						//				 "X-Powered-By: ASP.NET\r\n" +
						//				 "Date: Fri, 26 Apr 2013 20:15:30 GMT\r\n" +
						//				 "Content-Length: 10\r\n\r\n" +
						//				 "\r\n\r\n<h1>B</h1>";

						using (var swriter = new StreamWriter(stream))
						{
							swriter.Write(remoteResponse);
							swriter.Flush();
						}
					}

					proxyClient.Close();
				}
			}
		}

		private static string ReadAll2(StreamReader reader)
		{
			var req = new StringBuilder();

			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());
			req.AppendLine(reader.ReadLine());

			var rs = req.ToString();
			return rs;
		}

		private static string ReadAll(StreamReader reader)
		{
			var req = new StringBuilder();
			string f;
			while ((f = reader.ReadLine()) != "")
			{
				req.AppendLine(f);
			}

			var rs = req.ToString();
			return rs;
		}
	}
}