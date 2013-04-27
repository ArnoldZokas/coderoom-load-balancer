using System;
using System.IO;
using System.Net.Sockets;

namespace Coderoom.LoadBalancer
{
	public class TcpClientWrapper : ITcpClientWrapper
	{
		private readonly TcpClient _client;

		public TcpClientWrapper(TcpClient client)
		{
			_client = client;
		}

		public void Dispose()
		{
			_client.Close();
		}

		public Stream GetStream()
		{
			return _client.GetStream();
		}
	}

	public interface ITcpClientWrapper : IDisposable
	{
		Stream GetStream();
	}
}