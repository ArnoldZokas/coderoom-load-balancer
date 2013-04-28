using System;
using System.IO;
using System.Net;

namespace Coderoom.LoadBalancer
{
	public class RawHttpRequestParser : IRawHttpRequestParser
	{
		public RawHttpRequestParserResult ParseFromClientStream(Stream clientStream)
		{
			var result = new RawHttpRequestParserResult();

			using (var clientStreamReader = HttpProxyConfiguration.StreamReaderFactory(clientStream, true))
			{
				var line = clientStreamReader.ReadLine();
				result.RequestUri = GetRequestUri(line);

				var headers = new WebHeaderCollection();
				while (string.IsNullOrWhiteSpace(line = clientStreamReader.ReadLine()) == false)
				{
					var key = line.Substring(0, line.IndexOf(":", StringComparison.Ordinal));
					var value = line.Substring(key.Length + 2, line.Length - key.Length - 2);
					headers.Add(key, value);
				}
				result.RequestHeaders = headers;
			}

			return result;
		}

		static string GetRequestUri(string requestLine)
		{
			/* Request line format per HTTP specification http://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html#sec5.1
			 * 
			 *		REQUEST-LINE = Method Request-URI HTTP-Version CRLF
			 */
			const int pathFragmentPosition = 1;

			var requestLineFragments = requestLine.Split(' ');
			return requestLineFragments[pathFragmentPosition];
		}
	}

	public interface IRawHttpRequestParser
	{
		RawHttpRequestParserResult ParseFromClientStream(Stream clientStream);
	}
}