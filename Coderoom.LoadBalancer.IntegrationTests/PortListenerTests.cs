using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using Shouldly;

namespace Coderoom.LoadBalancer.IntegrationTests
{
	public class PortListenerTests
	{
		[TestFixture]
		public class when_port_listener_receives_connection
		{
			ManualResetEvent _event;
			PortListener _listener;
			TcpClient _tcpClient;
			Stream _clientStream;
			bool _eventRaised;

			[SetUp]
			public void SetUp()
			{
				_event = new ManualResetEvent(false);

				var endPoint = new IPEndPoint(IPAddress.Loopback, 17000);
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

			[TearDown]
			public void TearDown()
			{
				_tcpClient.Close();
				_listener.Stop();
			}

			[Test]
			public void it_raises_connection_established_event()
			{
				_eventRaised.ShouldBe(true);
			}

			[Test]
			public void it_opens_client_stream()
			{
				Assert.NotNull(_clientStream);
			}
		}
	}
}