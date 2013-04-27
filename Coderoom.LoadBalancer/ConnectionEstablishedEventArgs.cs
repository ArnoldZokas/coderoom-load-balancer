using System;

namespace Coderoom.LoadBalancer
{
	public class ConnectionEstablishedEventArgs : EventArgs
	{
		public ConnectionEstablishedEventArgs(ITcpClientWrapper client)
		{
			Client = client;
		}

		public ITcpClientWrapper Client { get; private set; }
	}
}