using System;
using System.IO;
using System.Net.Sockets;

namespace Coderoom.LoadBalancer.Abstractions
{
	public class TcpClientWrapper : ITcpClient
	{
		readonly TcpClient _client;

		public TcpClientWrapper(TcpClient client)
		{
			_client = client;
		}

		public Stream GetStream()
		{
			return _client.GetStream();
		}

		public void Dispose()
		{
			_client.Close();
		}
	}

	public interface ITcpClient : IDisposable
	{
		Stream GetStream();
	}
}