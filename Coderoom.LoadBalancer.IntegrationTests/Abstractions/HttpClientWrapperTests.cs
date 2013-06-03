using System.Net.Http;
using System.Threading.Tasks;
using Coderoom.LoadBalancer.Abstractions;
using NUnit.Framework;

namespace Coderoom.LoadBalancer.IntegrationTests.Abstractions
{
	public class HttpClientWrapperTests
	{
		[TestFixture]
		public class when_request_is_sent_requested : given_http_client_wrapper
		{
			HttpResponseMessage _response;

			[SetUp]
			public void SetUp()
			{
				var continuation = SendAsync("http://google.com/").ContinueWith(t => { _response = t.Result; });
				continuation.Wait();
			}

			[Test]
			public void it_should_return_response()
			{
				Assert.NotNull(_response);
			}

			[TearDown]
			public void TearDown()
			{
				Wrapper.Dispose();
				_response.Dispose();
			}
		}

		[TestFixture]
		public class when_request_to_a_resource_that_does_not_exist_is_sent : given_http_client_wrapper
		{
			HttpResponseMessage _response;

			[SetUp]
			public void SetUp()
			{
				var continuation = SendAsync("http://google.com/not-exists").ContinueWith(t => { _response = t.Result; });
				continuation.Wait();
			}

			[Test]
			public void it_should_return_response()
			{
				Assert.NotNull(_response);
			}

			[TearDown]
			public void TearDown()
			{
				Wrapper.Dispose();
				_response.Dispose();
			}
		}
	}

	public abstract class given_http_client_wrapper
	{
		protected HttpClientWrapper Wrapper;

		protected Task<HttpResponseMessage> SendAsync(string uri)
		{
			Wrapper = new HttpClientWrapper(new HttpClient());
			return Wrapper.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
		}
	}
}