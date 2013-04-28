using System;
using System.Net;
using Coderoom.LoadBalancer.Abstractions;
using NUnit.Framework;
using Shouldly;

namespace Coderoom.LoadBalancer.IntegrationTests.Abstractions
{
	public class WebRequestWrapperTests
	{
		[TestFixture]
		public class when_response_is_requested : given_web_request_wrapper
		{
			IWebResponse _response;

			[SetUp]
			public void SetUp()
			{
				base.SetUp("http://google.com/");

				_response = Wrapper.GetResponse();
			}

			[Test]
			public void it_should_return_response()
			{
				Assert.NotNull(_response);
			}

			[TearDown]
			public void TearDown()
			{
				_response.Dispose();
			}
		}

		[TestFixture]
		public class when_response_to_a_404_request_is_requested : given_web_request_wrapper
		{
			IWebResponse _response;

			[SetUp]
			public void SetUp()
			{
				base.SetUp("http://google.com/not-exists");

				_response = Wrapper.GetResponse();
			}

			[Test]
			public void it_should_return_response()
			{
				Assert.NotNull(_response);
			}

			[TearDown]
			public void TearDown()
			{
				_response.Dispose();
			}
		}

		[TestFixture]
		public class when_headers_are_added : given_web_request_wrapper
		{
			[SetUp]
			public void SetUp()
			{
				base.SetUp("http://google.com/");

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

			[Test]
			public void it_should_not_change_host()
			{
				Wrapper.WebRequest.Host.ShouldBe("google.com");
			}

			[Test]
			public void it_should_not_set_keep_alive()
			{
				Wrapper.WebRequest.KeepAlive.ShouldBe(true);
			}

			[Test]
			public void it_should_not_set_accept_encoding()
			{
				Wrapper.WebRequest.TransferEncoding.ShouldBe(null);
			}

			[Test]
			public void it_should_set_accept()
			{
				Wrapper.WebRequest.Accept.ShouldBe("*");
			}

			[Test]
			public void it_should_set_user_agent()
			{
				Wrapper.WebRequest.UserAgent.ShouldBe("xUnit");
			}

			[Test]
			public void it_should_set_if_modified_since()
			{
				Wrapper.WebRequest.IfModifiedSince.ShouldBe(new DateTime(2013, 04, 28, 21, 52, 00));
			}

			[Test]
			public void it_should_set_custom_header()
			{
				Wrapper.WebRequest.Headers["custom-1"].ShouldBe("value-1");
			}
		}
	}

	public abstract class given_web_request_wrapper
	{
		protected WebRequestWrapper Wrapper;

		protected void SetUp(string uri)
		{
			Wrapper = new WebRequestWrapper((HttpWebRequest)WebRequest.Create(new Uri(uri)));
		}
	}
}