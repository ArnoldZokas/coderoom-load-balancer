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
			WebResponse webResponse;

			try
			{
				webResponse = _webRequest.GetResponse();
			}
			catch (WebException wex)
			{
				webResponse = wex.Response;
			}

			return new WebResponseWrapper((HttpWebResponse)webResponse);
		}
	}

	public interface IWebRequest
	{
		IWebResponse GetResponse();
	}
}