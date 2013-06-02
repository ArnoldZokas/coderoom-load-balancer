using System;

namespace Coderoom.LoadBalancer.Service.Infrastructure
{
	public class Logger : ILogger
	{
		public void LogException(Exception exception)
		{
		}
	}

	public interface ILogger
	{
		void LogException(Exception exception);
	}
}