using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceProcess;
using Coderoom.LoadBalancer.Diagnostics.Logging;

namespace Coderoom.LoadBalancer.Service
{
	public partial class Host : ServiceBase
	{
		HttpProxy _httpProxy;
		ILogger _logger;

		public Host()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			_logger = new Logger();

			try
			{
				var endPoint = new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 80);
				var contentServers = new List<IPEndPoint>
					{
						new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081),
						new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8082)
					};

				var portListener = new PortListener(endPoint);
				_httpProxy = new HttpProxy(contentServers, portListener);
				_httpProxy.Start();
			}
			catch (Exception ex)
			{
				_logger.LogException(ex);
			}
		}

		protected override void OnStop()
		{
			try
			{
				_httpProxy.Stop();
			}
			catch (Exception ex)
			{
				_logger.LogException(ex);
			}
		}
	}
}