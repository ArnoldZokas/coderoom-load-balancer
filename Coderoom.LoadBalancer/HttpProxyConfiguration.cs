using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text;
using Coderoom.LoadBalancer.Abstractions;

namespace Coderoom.LoadBalancer
{
	[ExcludeFromCodeCoverage]
	public class HttpProxyConfiguration
	{
		static HttpProxyConfiguration()
		{
			ResetToDefault();
		}

		public static Func<Stream, bool, TextReader> StreamReaderFactory { get; set; }
		public static Func<IHttpClientWrapper> HttpClientFactory { get; set; }

		public static void ResetToDefault()
		{
			StreamReaderFactory = (stream, leaveOpen) => new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen);
			HttpClientFactory = () => new HttpClientWrapper(new HttpClient());
		}
	}
}