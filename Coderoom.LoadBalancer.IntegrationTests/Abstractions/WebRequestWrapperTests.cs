using System;
using System.Net;
using Coderoom.LoadBalancer.Abstractions;
using Xunit;

namespace Coderoom.LoadBalancer.IntegrationTests.Abstractions
{
	public class WebRequestWrapperTests
	{
		public class when_response_is_requested : given_web_request_wrapper, IDisposable
		{
			readonly IWebResponse _response;

			public when_response_is_requested() : base("http://google.com/")
			{
				_response = Wrapper.GetResponse();
			}

			[Fact]
			public void it_should_return_response()
			{
				Assert.NotNull(_response);
			}

			public void Dispose()
			{
				_response.Dispose();
			}
		}

		public class when_response_to_a_404_request_is_requested : given_web_request_wrapper, IDisposable
		{
			readonly IWebResponse _response;

			public when_response_to_a_404_request_is_requested() : base("http://google.com/not-exists")
			{
				_response = Wrapper.GetResponse();
			}

			[Fact]
			public void it_should_return_response()
			{
				Assert.NotNull(_response);
			}

			public void Dispose()
			{
				_response.Dispose();
			}
		}

		public class when_headers_are_added : given_web_request_wrapper
		{
			public when_headers_are_added() : base("http://google.com/")
			{
				var webHeaderCollection = new WebHeaderCollection
					{
						{"host", "example.com"},
						{"connection", "keep-alive"},
						{"accept-encoding", "gzip"},
						{"accept", "*"},
						{"user-agent", "xUnit"},
						{"if-modified-since", "28/04/2013 21:52"},
						{"custom-1", "value-1"}
					};
				Wrapper.AddHeaders(webHeaderCollection);
			}

			[Fact]
			public void it_should_not_change_host()
			{
				Assert.Equal("google.com", Wrapper.WebRequest.Host);
			}

			[Fact]
			public void it_should_not_set_keep_alive()
			{
				Assert.True(Wrapper.WebRequest.KeepAlive);
			}

			[Fact]
			public void it_should_not_set_accept_encoding()
			{
				Assert.Null(Wrapper.WebRequest.TransferEncoding);
			}

			[Fact]
			public void it_should_set_accept()
			{
				Assert.Equal("*", Wrapper.WebRequest.Accept);
			}

			[Fact]
			public void it_should_set_user_agent()
			{
				Assert.Equal("xUnit", Wrapper.WebRequest.UserAgent);
			}

			[Fact]
			public void it_should_set_if_modified_since()
			{
				Assert.Equal(new DateTime(2013, 04, 28, 21, 52, 00), Wrapper.WebRequest.IfModifiedSince);
			}

			[Fact]
			public void it_should_set_custom_header()
			{
				Assert.Equal("value-1", Wrapper.WebRequest.Headers["custom-1"]);
			}
		}
	}

	public abstract class given_web_request_wrapper
	{
		protected WebRequestWrapper Wrapper;

		protected given_web_request_wrapper(string uri)
		{
			Wrapper = new WebRequestWrapper((HttpWebRequest)WebRequest.Create(new Uri(uri)));
		}
	}
}