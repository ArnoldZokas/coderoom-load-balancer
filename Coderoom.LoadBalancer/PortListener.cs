using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Coderoom.LoadBalancer.Abstractions;

namespace Coderoom.LoadBalancer
{
	public class PortListener : IPortListener
	{
		readonly IPEndPoint _endPoint;
		readonly Thread _listenerThread;
		TcpListener _listener;
		bool _stopRequested;

		public PortListener(IPEndPoint endPoint)
		{
			_endPoint = endPoint;
			_listenerThread = new Thread(ListenForConnections);
		}

		public void Start()
		{
			_listener = new TcpListener(_endPoint);
			_listener.Start();
			_listenerThread.Start();
			OnStarted(EventArgs.Empty);
		}

		public event EventHandler<EventArgs> Started;
		public event EventHandler<ConnectionEstablishedEventArgs> ConnectionEstablished;

		public void Stop()
		{
			_stopRequested = true;
			_listenerThread.Abort();
			_listener.Stop();
		}

		void ListenForConnections()
		{
			while (_stopRequested == false)
			{
				if (!_listener.Pending())
					continue;

				var tcpClient = _listener.AcceptTcpClient();
				OnConnectionEstablished(new ConnectionEstablishedEventArgs(new TcpClientWrapper(tcpClient)));
			}
		}

		protected virtual void OnStarted(EventArgs e)
		{
			if (Started != null)
				Started(this, e);
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