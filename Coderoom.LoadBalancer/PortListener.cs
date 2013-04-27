using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Coderoom.LoadBalancer
{
	public class PortListener : IPortListener
	{
		private readonly IPEndPoint _endPoint;
		private readonly Thread _listenerThread;
		private bool _stopRequested;

		public PortListener(IPEndPoint endPoint)
		{
			_endPoint = endPoint;
			_listenerThread = new Thread(ListenForConnections);
		}

		public void Start()
		{
			_listenerThread.Start();
		}

		public event EventHandler<ConnectionEstablishedEventArgs> ConnectionEstablished;

		public void Stop()
		{
			_stopRequested = true;
			_listenerThread.Abort();
		}

		private void ListenForConnections()
		{
			var listener = new TcpListener(_endPoint);
			listener.Start();

			while (_stopRequested == false)
			{
				if (!listener.Pending())
					continue;

				var tcpClient = listener.AcceptTcpClient();
				OnConnectionEstablished(new ConnectionEstablishedEventArgs(new TcpClientWrapper(tcpClient)));
			}
		}

		protected virtual void OnConnectionEstablished(ConnectionEstablishedEventArgs e)
		{
			if (ConnectionEstablished != null)
				ConnectionEstablished(this, e);
		}
	}

	public interface IPortListener
	{
		event EventHandler<ConnectionEstablishedEventArgs> ConnectionEstablished;

		void Start();
	}
}