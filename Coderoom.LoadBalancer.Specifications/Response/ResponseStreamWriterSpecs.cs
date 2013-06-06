using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Coderoom.LoadBalancer.Response;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Coderoom.LoadBalancer.Specifications.Response
{
	public class ResponseStreamWriterSpecs
	{
		public class when_request_message_is_built_from_stream : given_a_response_stream_writer
		{
			static Mock<Stream> clientStream;
			static string capturedResponse;
			static HttpResponseMessage responseMessage;

			Establish context = () =>
				{
					clientStream = new Mock<Stream>();
					clientStream.Setup(x => x.CanRead).Returns(true);
					clientStream.Setup(x => x.CanWrite).Returns(true);
					clientStream.Setup(x => x.Write(Moq.It.IsAny<byte[]>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>()))
					            .Callback<byte[], int, int>((bytes, offset, count) => capturedResponse = Encoding.UTF8.GetString(bytes));

					responseMessage = new HttpResponseMessage
						{
							Version = new Version(1, 1),
							StatusCode = HttpStatusCode.OK,
							ReasonPhrase = "OK",
							Content = new StringContent("<p>content</p>")
						};
					responseMessage.Headers.Add("header-1", "value 1");
					responseMessage.Headers.Add("header-2", "value 2");
				};

			Because of = () => responseStreamWriter.WriteHttpResponseToClientStream(responseMessage, clientStream.Object);

			Cleanup after = () => HttpProxyConfiguration.ResetToDefault();

			It should_write_200_status_line = () => capturedResponse.ShouldContain("HTTP/1.1 200 OK");
			It should_write_http_headers = () => capturedResponse.ShouldContain("header-1: value 1\r\nheader-2: value 2\r\nContent-Type: text/plain");
			It should_write_content = () => capturedResponse.ShouldContain("<p>content</p>");
		}
	}

	public class given_a_response_stream_writer
	{
		protected static IResponseStreamWriter responseStreamWriter;

		Establish context = () => responseStreamWriter = new ResponseStreamWriter();
	}
}