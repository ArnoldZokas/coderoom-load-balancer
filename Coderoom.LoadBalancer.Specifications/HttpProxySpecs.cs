using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Coderoom.LoadBalancer.Abstractions;
using Coderoom.LoadBalancer.Request;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Coderoom.LoadBalancer.Specifications
{
	public class HttpProxySpecs
	{
		public class when_started : given_an_http_proxy
		{
			Because of = () => httpProxy.Start();

			It should_stop_listener = () => portListener.Verify(x => x.Start());
		}

		public class when_stopped : given_an_http_proxy
		{
			Because of = () => httpProxy.Stop();

			It should_stop_listener = () => portListener.Verify(x => x.Stop());
		}

		public class when_a_resource_is_requested : given_an_http_proxy
		{
			Establish context = () =>
				{
					proxiedResponseMessage = new HttpResponseMessage
						{
							Version = new Version(1, 1),
							StatusCode = HttpStatusCode.OK,
							ReasonPhrase = HttpStatusCode.OK.ToString(),
							Content = new StringContent("<p>content</p>")
						};
					proxiedResponseMessage.Headers.Add("header-1", "value 1");
					proxiedResponseMessage.Headers.Add("header-2", "value 2");

					var httpRequestMessage = new HttpRequestMessage();
					requestBuilder.Setup(x => x.BuildRequestFromRequestStream(Moq.It.IsAny<IPEndPoint>(), Moq.It.IsAny<Stream>())).Returns(httpRequestMessage);
				};

			Because of = () =>
				{
					var waitHandle = new AutoResetEvent(false);
					httpProxy.RequestProcessed += (sender, args) => waitHandle.Set();
					portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));
					waitHandle.WaitOne();
				};

			It should_make_internal_request = () => requestBuilder.Verify(x => x.BuildRequestFromRequestStream(Moq.It.IsAny<IPEndPoint>(), Moq.It.IsAny<Stream>()));
			It should_return_200_status_line = () => capturedResponse.ShouldContain("HTTP/1.1 200 OK");
			It should_return_http_headers = () => capturedResponse.ShouldContain("header-1: value 1\r\nheader-2: value 2");
			It should_return_body = () => capturedResponse.ShouldContain("<p>content</p>");
		}
	}

	public class given_an_http_proxy
	{
		protected static HttpProxy httpProxy;
		protected static Mock<IPortListener> portListener;
		protected static Mock<ITcpClient> tcpClientWrapper;
		protected static Mock<Stream> tcpClientStream;
		protected static string capturedResponse;
		protected static HttpResponseMessage proxiedResponseMessage;
		protected static Mock<IRequestBuilder> requestBuilder;
		protected static Mock<IHttpClientWrapper> httpClient;

		Establish context = () =>
			{
				var ipEndPoints = new List<IPEndPoint> {new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081)};
				portListener = new Mock<IPortListener>();

				httpClient = new Mock<IHttpClientWrapper>();
				httpClient.Setup(x => x.SendAsync(Moq.It.IsAny<HttpRequestMessage>())).Returns(new Task<HttpResponseMessage>(() => proxiedResponseMessage));

				HttpProxyConfiguration.HttpClientFactory = () => httpClient.Object;

				tcpClientStream = new Mock<Stream>();
				tcpClientStream
					.Setup(x => x.Write(Moq.It.IsAny<byte[]>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>()))
					.Callback<byte[], int, int>((bytes, offset, count) => capturedResponse = Encoding.UTF8.GetString(bytes));
				tcpClientStream.Setup(x => x.CanRead).Returns(true);
				tcpClientStream.Setup(x => x.CanWrite).Returns(true);

				tcpClientWrapper = new Mock<ITcpClient>();
				tcpClientWrapper.Setup(x => x.GetStream()).Returns(tcpClientStream.Object);

				requestBuilder = new Mock<IRequestBuilder>();
				httpProxy = new HttpProxy(ipEndPoints, portListener.Object, requestBuilder.Object);
				httpProxy.Start();
			};

		Cleanup after = () => HttpProxyConfiguration.ResetToDefault();
	}
}