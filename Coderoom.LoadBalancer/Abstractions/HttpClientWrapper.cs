using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Coderoom.LoadBalancer.Abstractions
{
	public class HttpClientWrapper : IHttpClientWrapper
	{
		readonly HttpClient _httpClient;

		public HttpClientWrapper(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage)
		{
			return _httpClient.SendAsync(httpRequestMessage);
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}

	public interface IHttpClientWrapper : IDisposable
	{
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage);
	}
}