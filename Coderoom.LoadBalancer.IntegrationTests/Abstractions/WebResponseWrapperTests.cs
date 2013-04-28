using System;
using System.IO;
using System.Net;
using Coderoom.LoadBalancer.Abstractions;
using Xunit;

namespace Coderoom.LoadBalancer.IntegrationTests.Abstractions
{
	public class WebResponseWrapperTests
	{
		public class when_response_stream_is_requested : given_web_response_wrapper, IDisposable
		{
			readonly Stream _stream;

			public when_response_stream_is_requested() : base("http://google.com/")
			{
				_stream = Wrapper.GetResponseStream();
			}

			[Fact]
			public void it_should_return_response()
			{
				Assert.NotNull(_stream);
			}

			public void Dispose()
			{
				_stream.Dispose();
				Wrapper.Dispose();
			}
		}

		public class when_status_line_is_requested : given_web_response_wrapper, IDisposable
		{
			readonly string _statusLine;

			public when_status_line_is_requested() : base("http://google.com/")
			{
				_statusLine = Wrapper.GetStatusLine();
			}

			[Fact]
			public void it_should_return_status_line()
			{
				Assert.Equal("HTTP/1.1 200 OK", _statusLine);
			}

			public void Dispose()
			{
				Wrapper.Dispose();
			}
		}

		public class when_headers_are_requested : given_web_response_wrapper, IDisposable
		{
			readonly WebHeaderCollection _headers;

			public when_headers_are_requested() : base("http://google.com/")
			{
				_headers = Wrapper.GetHeaders();
			}

			[Fact]
			public void it_should_return_headers()
			{
				Assert.True(_headers.Count > 0);
			}

			public void Dispose()
			{
				Wrapper.Dispose();
			}
		}
	}

	public abstract class given_web_response_wrapper
	{
		protected WebResponseWrapper Wrapper;

		protected given_web_response_wrapper(string uri)
		{
			Wrapper = new WebResponseWrapper((HttpWebResponse)WebRequest.Create(new Uri(uri)).GetResponse());
		}
	}
}