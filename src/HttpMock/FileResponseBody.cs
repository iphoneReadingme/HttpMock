using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;

namespace HttpMock
{
	class FileResponseBody :  IResponse
	{
		private readonly string _filepath;
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private IDictionary<string, string> _requestHeaders;

		public FileResponseBody(string filepath) {
			_filepath = filepath;
		}

		public IDisposable Connect(Stream outputStream) {
			var fileInfo = new FileInfo(_filepath);
			using(FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read)) {
				var buffer = new byte[fileInfo.Length];
				fileStream.Read(buffer, 0, (int) fileInfo.Length);
				int length = (int) fileInfo.Length;
				int offset = 0;

				if(_requestHeaders.ContainsKey(HttpRequestHeader.Range.ToString())) {
					string range = _requestHeaders[HttpRequestHeader.Range.ToString()];
					Regex rangeEx = new Regex(@"bytes=([\d]*)-([\d]*)");
					if(rangeEx.IsMatch(range)) {
						int from = Convert.ToInt32(rangeEx.Match(range).Groups[1].Value);
						int to = Convert.ToInt32(rangeEx.Match(range).Groups[2].Value);
						offset = from;
						length = (to - from) +1;
					}
				}
				
				outputStream.WriteAsync(buffer, offset, length);
				
				_log.DebugFormat("Wrote {0} bytes to buffer", buffer.Length);
				
				return null;
			}
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders) {
			_requestHeaders = requestHeaders;
		}
	}

	public interface IResponse {
		void SetRequestHeaders(IDictionary<string, string> requestHeaders);
		IDisposable Connect(Stream outputStream);
	}
}