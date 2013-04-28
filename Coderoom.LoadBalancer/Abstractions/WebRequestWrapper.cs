using System.Net;

namespace Coderoom.LoadBalancer.Abstractions
{
	public class WebRequestWrapper : IWebRequest
	{
		readonly WebRequest _webRequest;

		public WebRequestWrapper(WebRequest webRequest)
		{
			_webRequest = webRequest;
		}

		public IWebResponse GetResponse()
		{
			return new WebResponseWrapper((HttpWebResponse)_webRequest.GetResponse());
		}
	}

	public interface IWebRequest
	{
		IWebResponse GetResponse();
	}
}