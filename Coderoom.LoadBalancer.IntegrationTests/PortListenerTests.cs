using System;
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
			bool _eventRaised;

			public when_port_listener_receives_connection()
			{
				_event = new ManualResetEvent(false);

				var endPoint = new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 17000);
				_listener = new PortListener(endPoint);
				_listener.ConnectionEstablished += (sender, args) =>
					{
						_eventRaised = true;
						_event.Set();
						args.Client.Dispose();
					};
				_listener.Start();

				_tcpClient = new TcpClient();
				_tcpClient.Connect(endPoint);
			}

			public void Dispose()
			{
				_tcpClient.Close();
				_listener.Stop();
			}

			[Fact]
			public void it_raises_connection_established_event()
			{
				WaitHandle.WaitAll(new[] {_event});
				Assert.True(_eventRaised);
			}
		}
	}
}