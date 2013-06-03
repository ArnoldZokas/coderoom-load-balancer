using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Coderoom.LoadBalancer.Request
{
	public class RequestBuilder : IRequestBuilder
	{
		public HttpRequestMessage BuildRequestFromRequestStream(IPEndPoint endPoint, Stream stream)
		{
			var httpRequestMessage = new HttpRequestMessage();

			using (var clientStreamReader = HttpProxyConfiguration.StreamReaderFactory(stream, true))
			{
				httpRequestMessage.RequestUri = BuildAbsoluteRequestUri(endPoint, clientStreamReader.ReadLine());
				CopyHeaders(httpRequestMessage, clientStreamReader);
				httpRequestMessage.Content = CopyContent();
				
				return httpRequestMessage;
			}
		}

		static Uri BuildAbsoluteRequestUri(IPEndPoint endPoint, string line)
		{
			/* Request line format per HTTP specification http://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html#sec5.1
			 * 
			 *		REQUEST-LINE = Method Request-URI HTTP-Version CRLF
			 */

			var absoluteUri = new Uri(string.Format("{0}{1}{2}", Uri.UriSchemeHttp, Uri.SchemeDelimiter, endPoint), UriKind.Absolute);
			var relativeUri = line.Split(' ')[1];
			return new Uri(absoluteUri, relativeUri);
		}

		static void CopyHeaders(HttpRequestMessage httpRequestMessage, TextReader clientStreamReader)
		{
			string line;
			while (string.IsNullOrWhiteSpace(line = clientStreamReader.ReadLine()) == false)
			{
				var key = line.Substring(0, line.IndexOf(":", StringComparison.OrdinalIgnoreCase));
				var value = line.Substring(key.Length + 2, line.Length - key.Length - 2);
				httpRequestMessage.Headers.Add(key, value);
			}
		}

		static StringContent CopyContent()
		{
			return new StringContent(string.Empty);
		}
	}

	public interface IRequestBuilder
	{
		HttpRequestMessage BuildRequestFromRequestStream(IPEndPoint endPoint, Stream stream);
	}
}