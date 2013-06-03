using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Coderoom.LoadBalancer.Request;
using Coderoom.LoadBalancer.Specifications.Utilities;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Coderoom.LoadBalancer.Specifications.Request
{
	public class RequestMessageBuilderSpecs
	{
		public class when_request_message_is_built_from_stream : given_a_request_message_builder
		{
			static IPEndPoint ipEndPoint;
			static Mock<MemoryStream> memoryStream;
			static HttpRequestMessage requestMessage;

			Establish context = () =>
				{
					ipEndPoint = new IPEndPoint(IPAddress.Loopback, 80);
					memoryStream = new Mock<MemoryStream>();

					var textReader = new Mock<TextReader>();
					textReader.MockRawRequestContent(new[]
						{
							"GET / HTTP/1.1",
							"Accept-Charset: utf-8",
							"Accept-Encoding: gzip"
						});
					HttpProxyConfiguration.StreamReaderFactory = (stream, leaveOpen) => textReader.Object;
				};

			Because of = () => requestMessage = requestMessageBuilder.BuildRequestFromRequestStream(ipEndPoint, memoryStream.Object);

			Cleanup after = () => HttpProxyConfiguration.ResetToDefault();

			It should_use_get_method = () => requestMessage.Method.ShouldEqual(HttpMethod.Get);
			It should_target_root = () => requestMessage.RequestUri.ShouldEqual(new Uri("http://127.0.0.1/", UriKind.Absolute));
			It should_contain_2_headers = () => requestMessage.Headers.Count().ShouldEqual(2);
			It should_contain_accept_charset_header = () => requestMessage.Headers.AcceptCharset.First().Value.ShouldEqual("utf-8");
			It should_contain_accept_encoding_header = () => requestMessage.Headers.AcceptEncoding.First().Value.ShouldEqual("gzip");
		}
	}

	public class given_a_request_message_builder
	{
		protected static IRequestMessageBuilder requestMessageBuilder;

		Establish context = () => requestMessageBuilder = new RequestMessageBuilder();
	}
}