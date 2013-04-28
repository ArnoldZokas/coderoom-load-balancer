﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
			Establish context = () => webResponse.Setup(x => x.GetStatusLine()).Returns("HTTP/1.1 200 OK");

			Because of = () =>
				{
					webResponse.MockHttpHeaders(new WebHeaderCollection
						{
							{"header-1", "value 1"},
							{"header-2", "value 2"}
						});

					textReader.MockRawRequestContent(new[]
						{
							"GET / HTTP/1.1",
							"header-1: value 1",
							"header-2: value 2"
						});

					textReader.Setup(x => x.ReadToEnd()).Returns("<p>content</p>");

					portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));
				};

			It should_transmit_http_headers = () => webRequest.Verify(x => x.AddHeaders(Moq.It.Is<WebHeaderCollection>(headers => headers.Count == 2)));
			It should_return_200_status_line = () => capturedResponse.ShouldContain("HTTP/1.1 200 OK");
			It should_return_http_headers = () => capturedResponse.ShouldContain("header-1: value 1\r\nheader-2: value 2");
			It should_return_body = () => capturedResponse.ShouldContain("<p>content</p>");
		}

		public class when_a_missing_resource_is_requested : given_an_http_proxy
		{
			Establish context = () => webResponse.Setup(x => x.GetStatusLine()).Returns("HTTP/1.1 404 Not Found");

			Because of = () =>
				{
					textReader.MockRawRequestContent(new[]
						{
							"GET / HTTP/1.1"
						});

					portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));
				};

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
		protected static Mock<IWebRequest> webRequest;
		protected static Mock<IWebResponse> webResponse;
		protected static Mock<Stream> webResponseStream;

		Establish context = () =>
			{
				var ipEndPoints = new List<IPEndPoint> {new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081)};
				portListener = new Mock<IPortListener>();
				textReader = new Mock<TextReader>();
				webResponse = new Mock<IWebResponse>();
				webResponseStream = new Mock<Stream>();

				webResponse.Setup(x => x.GetResponseStream()).Returns(webResponseStream.Object);
				webResponse.Setup(x => x.GetHeaders()).Returns(() => new WebHeaderCollection());
				webRequest = new Mock<IWebRequest>();
				webRequest.Setup(x => x.GetResponse()).Returns(webResponse.Object);

				HttpProxyConfiguration.StreamReaderFactory = (stream, leaveOpen) => textReader.Object;
				HttpProxyConfiguration.WebRequestFactory = uri => webRequest.Object;

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