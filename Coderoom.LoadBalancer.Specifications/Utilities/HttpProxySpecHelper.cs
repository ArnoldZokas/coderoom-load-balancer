using System.IO;
using Moq;

namespace Coderoom.LoadBalancer.Specifications.Utilities
{
	public static class HttpProxySpecHelper
	{
		public static void MockRawRequestContent(this Mock<TextReader> textReader, string[] requestContent)
		{
			var requestIndex = -1;
			textReader.Setup(x => x.ReadLine()).Returns(() =>
				{
					requestIndex++;
					return requestIndex < requestContent.Length ? requestContent[requestIndex] : string.Empty;
				});
		}
	}
}