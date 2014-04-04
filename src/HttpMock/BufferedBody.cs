using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpMock
{
	internal class BufferedBody : IResponse
	{
		private ArraySegment<byte> data;

		public BufferedBody(string data) : this(data, Encoding.UTF8)
		{
		}

		public BufferedBody(string data, Encoding encoding) : this(encoding.GetBytes(data))
		{
		}

		public BufferedBody(byte[] data) : this(new ArraySegment<byte>(data))
		{
		}

		public BufferedBody(ArraySegment<byte> data)
		{
			this.data = data;
			Length = data.Count;
		}

		public int Length { get; private set; }

		public IDisposable Connect(Stream outputStream)
		{
			byte[] buffer = data.Array;
			outputStream.WriteAsync(buffer, 0, buffer.Length);
			return null;
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders)
		{
		}
	}

	internal class NoBody : IResponse
	{
		public IDisposable Connect(Stream outputStream)
		{
			return null;
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders)
		{
		}
	}
}