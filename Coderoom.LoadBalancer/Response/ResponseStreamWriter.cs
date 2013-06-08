using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Coderoom.LoadBalancer.Utilities;

namespace Coderoom.LoadBalancer.Response
{
	public class ResponseStreamWriter : IResponseStreamWriter
	{
		public void WriteHttpResponseToClientStream(HttpResponseMessage responseMessage, Stream clientStream)
		{
			var responseBuilder = new StringBuilder();
			WriteResponseLine(responseMessage, responseBuilder);
			CopyHeaders(responseMessage, responseBuilder);
			CopyContent(responseMessage, responseBuilder);

			var response = responseBuilder.ToString();
			using (var streamWriter = new StreamWriter(clientStream))
				streamWriter.Write(response);
		}

		static void WriteResponseLine(HttpResponseMessage responseMessage, StringBuilder responseBuilder)
		{
			var responseLine = "HTTP/{0}.{1} {2} {3}".Fmt(responseMessage.Version.Major, responseMessage.Version.Minor, (int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
			responseBuilder.AppendLine(responseLine);
		}

		static void CopyHeaders(HttpResponseMessage responseMessage, StringBuilder responseBuilder)
		{
			var responseHeaders = responseMessage.Headers.Union(responseMessage.Content.Headers);
			foreach (var header in responseHeaders)
			{
				foreach (var headerValue in header.Value)
				{
					responseBuilder.AppendLine(string.Format("{0}: {1}", header.Key, headerValue));
				}
			}
		}

		static void CopyContent(HttpResponseMessage responseMessage, StringBuilder responseBuilder)
		{
			var content = responseMessage.Content.ReadAsStringAsync().Result;

			responseBuilder.AppendLine();
			responseBuilder.Append(content);
		}
	}

	public interface IResponseStreamWriter
	{
		void WriteHttpResponseToClientStream(HttpResponseMessage responseMessage, Stream clientStream);
	}
}