using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace Coderoom.LoadBalancer.IntegrationTests
{
	public class PortListenerTests
	{
		public class when_port_listener_receives_connection : IDisposable
		{
			readonly ManualResetEvent _event;
			readonly PortListener _listener;
			readonly TcpClient _tcpClient;
			Stream _clientStream;
			bool _eventRaised;

			public when_port_listener_receives_connection()
			{
				_event = new ManualResetEvent(false);

				var endPoint = new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 17000);
				_listener = new PortListener(endPoint);
				_listener.ConnectionEstablished += (sender, args) =>
					{
						_eventRaised = true;
						_clientStream = args.Client.GetStream();
						args.Client.Dispose();
						_event.Set();
					};

				_tcpClient = new TcpClient();
				_listener.Started += (sender, args) => _tcpClient.Connect(endPoint);
				_listener.Start();

				WaitHandle.WaitAll(new WaitHandle[] {_event});
			}

			public void Dispose()
			{
				_tcpClient.Close();
				_listener.Stop();
			}

			[Fact]
			public void it_raises_connection_established_event()
			{
				Assert.True(_eventRaised);
			}

			[Fact]
			public void it_raises_returns_client_stream()
			{
				Assert.NotNull(_clientStream);
			}
		}
	}
}