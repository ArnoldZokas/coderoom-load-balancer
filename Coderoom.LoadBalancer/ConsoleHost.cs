using System.Collections.Generic;
using System.Net;

namespace Coderoom.LoadBalancer
{
	internal class ConsoleHost
	{
		public static void Main()
		{
			var servers = new List<IPEndPoint>
				{
					new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081),
					new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8082)
				};

			var httpProxy = new HttpProxy(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 81), servers);
			httpProxy.Start();
		}
	}
}