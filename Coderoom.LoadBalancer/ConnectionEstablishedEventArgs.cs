using System;
using Coderoom.LoadBalancer.Abstractions;

namespace Coderoom.LoadBalancer
{
	public class ConnectionEstablishedEventArgs : EventArgs
	{
		public ConnectionEstablishedEventArgs(ITcpClient client)
		{
			Client = client;
		}

		public ITcpClient Client { get; private set; }
	}
}