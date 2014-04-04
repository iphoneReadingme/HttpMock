using System.Collections.Generic;
using System.Collections.Specialized;

namespace HttpMock
{
	public class HttpRequestHead
	{
		public string Method { get; set; }
		public string Uri { get; set; }
		public IDictionary<string, string> Headers { get; set; }
		public string Body { get; set; }
		public IDictionary<string, string> QueryString { get; set; }
		public bool HasEntityBody { get; set; }
	}
}