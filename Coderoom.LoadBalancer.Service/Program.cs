using System.ServiceProcess;

namespace Coderoom.LoadBalancer.Service
{
	internal static class Program
	{
		static void Main()
		{
			var servicesToRun = new ServiceBase[] { new Host() };
			ServiceBase.Run(servicesToRun);
		}
	}
}