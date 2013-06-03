using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Coderoom.LoadBalancer.Abstractions;
using Coderoom.LoadBalancer.Specifications.Utilities;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Coderoom.LoadBalancer.Specifications
{
	public class HttpProxySpecs
	{
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

					textReader.MockRawRequestContent(new[]
						{
							"GET / HTTP/1.1",
							"header-1: value 1",
							"header-2: value 2"
						});
				};

			Because of = () =>
				{
					var waitHandle = new AutoResetEvent(false);
					httpProxy.RequestProcessed += (sender, args) => waitHandle.Set();
					portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));
					waitHandle.WaitOne();
				};

			It should_make_get_request = () => httpClient.Verify(x => x.SendAsync(Moq.It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get)));
			It should_make_request_to_root = () => httpClient.Verify(x => x.SendAsync(Moq.It.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/")));
			It should_transmit_http_headers = () => httpClient.Verify(x => x.SendAsync(Moq.It.Is<HttpRequestMessage>(m => m.Headers.Count() == 2)));
			It should_return_200_status_line = () => capturedResponse.ShouldContain("HTTP/1.1 200 OK");
			It should_return_http_headers = () => capturedResponse.ShouldContain("header-1: value 1\r\nheader-2: value 2");
			It should_return_body = () => capturedResponse.ShouldContain("<p>content</p>");
		}

		public class when_a_missing_resource_is_requested : given_an_http_proxy
		{
			Establish context = delegate
				{
					proxiedResponseMessage = new HttpResponseMessage
						{
							Version = new Version(1, 1),
							StatusCode = HttpStatusCode.NotFound,
							ReasonPhrase = "Not Found",
							Content = new StringContent(string.Empty)
						};

					textReader.MockRawRequestContent(new[]
						{
							"GET / HTTP/1.1"
						});
				};

			Because of = () => portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));

			It should_return_404_status_line = () => capturedResponse.ShouldContain("HTTP/1.1 404 Not Found");
		}

		public class when_stopped : given_an_http_proxy
		{
			Because of = () => httpProxy.Stop();

			It should_stop_listener = () => portListener.Verify(x => x.Stop());
		}
	}

	public class given_an_http_proxy
	{
		protected static HttpProxy httpProxy;
		protected static Mock<IPortListener> portListener;
		protected static Mock<ITcpClient> tcpClientWrapper;
		protected static Mock<Stream> tcpClientStream;
		protected static string capturedResponse;
		protected static Mock<TextReader> textReader;
		protected static Mock<IHttpClientWrapper> httpClient;
		protected static HttpResponseMessage proxiedResponseMessage;
		protected static Mock<Stream> webResponseStream;

		Establish context = () =>
			{
				var ipEndPoints = new List<IPEndPoint> {new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081)};
				portListener = new Mock<IPortListener>();
				textReader = new Mock<TextReader>();
				webResponseStream = new Mock<Stream>();

				httpClient = new Mock<IHttpClientWrapper>();
				httpClient.Setup(x => x.SendAsync(Moq.It.IsAny<HttpRequestMessage>())).Returns(new Task<HttpResponseMessage>(() => proxiedResponseMessage));

				HttpProxyConfiguration.StreamReaderFactory = (stream, leaveOpen) => textReader.Object;
				HttpProxyConfiguration.HttpClientFactory = () => httpClient.Object;

				tcpClientStream = new Mock<Stream>();
				tcpClientStream
					.Setup(x => x.Write(Moq.It.IsAny<byte[]>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>()))
					.Callback<byte[], int, int>((bytes, offset, count) => capturedResponse = Encoding.UTF8.GetString(bytes));
				tcpClientStream.Setup(x => x.CanRead).Returns(true);
				tcpClientStream.Setup(x => x.CanWrite).Returns(true);

				tcpClientWrapper = new Mock<ITcpClient>();
				tcpClientWrapper.Setup(x => x.GetStream()).Returns(tcpClientStream.Object);

				httpProxy = new HttpProxy(ipEndPoints, portListener.Object);
				httpProxy.Start();
			};

		Cleanup after = () => HttpProxyConfiguration.ResetToDefault();
	}
}