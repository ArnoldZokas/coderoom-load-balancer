﻿#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using Coderoom.LoadBalancer.Abstractions;

namespace Coderoom.LoadBalancer
{
	[ExcludeFromCodeCoverage]
	internal class DebugConsoleHost
	{
		public static void Main()
		{
			var endPoint = new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 81);
			var contentServers = new List<IPEndPoint>
				{
					new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081),
					new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8082)
				};

			Console.WriteLine("SIMPLE LOAD BALANCER");
			Console.WriteLine("====================");
			Console.WriteLine("Configuration:");
			Console.WriteLine("    Target: {0}:{1}", endPoint.Address, endPoint.Port);
			Console.WriteLine("    Content servers:");
			foreach (var contentServer in contentServers)
				Console.WriteLine("        * {0}:{1}", contentServer.Address, contentServer.Port);

			var portListener = new PortListener(endPoint);
			var httpProxy = new HttpProxy(contentServers, portListener, (stream, leaveOpen) => new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen), uri => new WebRequestWrapper(WebRequest.Create(uri)));
			httpProxy.Start();

			Console.WriteLine();
			Console.WriteLine("Load Balancer started...");
			Console.WriteLine();
		}
	}
}

#endif