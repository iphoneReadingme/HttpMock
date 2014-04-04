using System.Collections.Generic;

namespace HttpMock
{
	public class HttpResponseHead
	{
		public string Status { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public string ContentType { get; set; }
		public long ContentLength { get; set; }
	}
}