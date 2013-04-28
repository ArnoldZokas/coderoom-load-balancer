using System.Net;

namespace Coderoom.LoadBalancer
{
	public class RawHttpRequestParserResult
	{
		public string RequestUri { get; set; }
		public WebHeaderCollection RequestHeaders { get; set; }
	}
}