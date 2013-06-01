using System;
using System.IO;

namespace Coderoom.LoadBalancer.Diagnostics.Logging
{
	public class Logger : ILogger
	{
		public void LogException(Exception exception)
		{
			File.WriteAllText("C:\\Deployment\\log.txt", exception.Message + " || " + exception.StackTrace);
		}
	}

	public interface ILogger
	{
		void LogException(Exception exception);
	}
}