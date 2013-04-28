using System;
using System.IO;
using System.Net;
using Coderoom.LoadBalancer.Abstractions;
using NUnit.Framework;
using Shouldly;

namespace Coderoom.LoadBalancer.IntegrationTests.Abstractions
{
	public class WebResponseWrapperTests
	{
		[TestFixture]
		public class when_response_stream_is_requested : given_web_response_wrapper, IDisposable
		{
			readonly Stream _stream;

			public when_response_stream_is_requested()
			{
				base.SetUp("http://google.com/");

				_stream = Wrapper.GetResponseStream();
			}

			[Test]
			public void it_should_return_response()
			{
				_stream.ShouldNotBe(null);
			}

			public void Dispose()
			{
				_stream.Dispose();
				Wrapper.Dispose();
			}
		}

		[TestFixture]
		public class when_status_line_is_requested : given_web_response_wrapper, IDisposable
		{
			readonly string _statusLine;

			public when_status_line_is_requested()
			{
				base.SetUp("http://google.com/");

				_statusLine = Wrapper.GetStatusLine();
			}

			[Test]
			public void it_should_return_status_line()
			{
				_statusLine.ShouldBe("HTTP/1.1 200 OK");
			}

			public void Dispose()
			{
				Wrapper.Dispose();
			}
		}

		[TestFixture]
		public class when_headers_are_requested : given_web_response_wrapper, IDisposable
		{
			readonly WebHeaderCollection _headers;

			public when_headers_are_requested()
			{
				base.SetUp("http://google.com/");

				_headers = Wrapper.GetHeaders();
			}

			[Test]
			public void it_should_return_headers()
			{
				_headers.Count.ShouldBeGreaterThan(0);
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

		protected void SetUp(string uri)
		{
			Wrapper = new WebResponseWrapper((HttpWebResponse)WebRequest.Create(new Uri(uri)).GetResponse());
		}
	}
}