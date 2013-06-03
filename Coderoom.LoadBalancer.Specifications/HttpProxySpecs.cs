using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Coderoom.LoadBalancer.Abstractions;
using Coderoom.LoadBalancer.Request;
using Coderoom.LoadBalancer.Response;
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
					var httpRequestMessage = new HttpRequestMessage();
					requestMessageBuilder.Setup(x => x.BuildRequestFromRequestStream(Moq.It.IsAny<IPEndPoint>(), Moq.It.IsAny<Stream>())).Returns(httpRequestMessage);
				};

			Because of = () =>
				{
					var tcpClientWrapper = new Mock<ITcpClient>();
					portListener.Raise(x => x.ConnectionEstablished += null, new ConnectionEstablishedEventArgs(tcpClientWrapper.Object));
				};

			It should_make_internal_request = () => requestMessageBuilder.Verify(x => x.BuildRequestFromRequestStream(Moq.It.IsAny<IPEndPoint>(), Moq.It.IsAny<Stream>()));
			It should_write_internal_response_to_client_stream = () => responseStreamWriter.Verify(x => x.WriteHttpResponseToClientStream(Moq.It.IsAny<HttpResponseMessage>(), Moq.It.IsAny<Stream>()));
		}
	}

	public class given_an_http_proxy
	{
		protected static HttpProxy httpProxy;
		protected static Mock<IPortListener> portListener;
		protected static Mock<Stream> tcpClientStream;
		protected static string capturedResponse;
		protected static HttpResponseMessage proxiedResponseMessage;
		protected static Mock<IRequestMessageBuilder> requestMessageBuilder;
		protected static Mock<IResponseStreamWriter> responseStreamWriter;
		protected static Mock<IHttpClientWrapper> httpClient;

		Establish context = () =>
			{
				var ipEndPoints = new List<IPEndPoint> {new IPEndPoint(IPAddress.Loopback, 80)};

				portListener = new Mock<IPortListener>();

				httpClient = new Mock<IHttpClientWrapper>();
				httpClient.Setup(x => x.SendAsync(Moq.It.IsAny<HttpRequestMessage>())).Returns(() =>
					{
						var t = Task.Run(() => new HttpResponseMessage());
						t.Wait();
						return t;
					});
				HttpProxyConfiguration.HttpClientFactory = () => httpClient.Object;

				requestMessageBuilder = new Mock<IRequestMessageBuilder>();
				responseStreamWriter = new Mock<IResponseStreamWriter>();

				httpProxy = new HttpProxy(ipEndPoints, portListener.Object, requestMessageBuilder.Object, responseStreamWriter.Object);
				httpProxy.Start();
			};

		Cleanup after = () => HttpProxyConfiguration.ResetToDefault();
	}
}