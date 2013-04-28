using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using Coderoom.LoadBalancer.Abstractions;

namespace Coderoom.LoadBalancer
{
	public class HttpProxyConfiguration
	{
		static HttpProxyConfiguration()
		{
			ResetToDefault();
		}

		public static Func<Stream, bool, TextReader> StreamReaderFactory { get; set; }
		public static Func<Uri, IWebRequest> WebRequestFactory { get; set; }
		public static IRawHttpRequestParser RawHttpRequestParser { get; set; }

		[ExcludeFromCodeCoverage]
		public static void ResetToDefault()
		{
			StreamReaderFactory = (stream, leaveOpen) => new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen);
			WebRequestFactory = uri => new WebRequestWrapper((HttpWebRequest)WebRequest.Create(uri));
			RawHttpRequestParser = new RawHttpRequestParser();
		}
	}
}