using System;
using System.Diagnostics.CodeAnalysis;
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

		public void Dispose()
		{
			_client.Close();
		}

		public Stream GetStream()
		{
			return _client.GetStream();
		}
	}

	public interface ITcpClient : IDisposable
	{
		Stream GetStream();
	}
}