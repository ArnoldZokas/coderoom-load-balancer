﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Coderoom.LoadBalancer.Abstractions;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Coderoom.LoadBalancer.Specifications
{
	public class HttpProxySpecs
	{
		public class when_a_missing_resource_is_requested : given_an_http_proxy
		{
			Establish context = () => webResponse.Setup(x => x.GetStatusLine()).Returns("HTTP/1.1 404 Not Found");

			Because of = () =>
				{
					httpProxy.Start();

					textReader.Setup(x => x.ReadLine()).Returns("GET / HTTP/1.1");

					portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));
				};

			It should_return_response_404_status_line = () => capturedResponse.ShouldContain("HTTP/1.1 404 Not Found");
		}

		public class when_a_resource_is_requested : given_an_http_proxy
		{
			Establish context = () => webResponse.Setup(x => x.GetStatusLine()).Returns("HTTP/1.1 200 OK");

			Because of = () =>
				{
					httpProxy.Start();

					textReader.Setup(x => x.ReadLine()).Returns("GET / HTTP/1.1");
					textReader.Setup(x => x.ReadToEnd()).Returns("<p>content</p>");

					portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));
				};

			It should_return_response_body = () => capturedResponse.ShouldContain("<p>content</p>");
			It should_return_response_status_line = () => capturedResponse.ShouldContain("HTTP/1.1 200 OK");
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

		Cleanup after = () => HttpProxyConfiguration.ResetToDefault();

		Establish context = () =>
			{
				var ipEndPoints = new List<IPEndPoint> {new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 8081)};
				portListener = new Mock<IPortListener>();
				textReader = new Mock<TextReader>();
				webResponse = new Mock<IWebResponse>();
				webResponseStream = new Mock<Stream>();

				webResponse.Setup(x => x.GetResponseStream()).Returns(webResponseStream.Object);
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
			};
	}
}