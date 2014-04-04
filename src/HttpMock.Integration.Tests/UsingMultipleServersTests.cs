using System;
using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingMultipleServersTests
	{
		[Test, Repeat(3)]
		public void Should_stubs_on_different_ports_each_time()
		{
			string expected = "expected response";
			var hostUrl = HostHelper.GenerateAHostUrlForAStubServerWith("app");

			HttpMockRepository.At(hostUrl)
				.Stub(x => x.Get("/app/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();

			Assert.That(wc.DownloadString(string.Format("{0}/endpoint", hostUrl)), Is.EqualTo(expected));
		}
	}


	[TestFixture]
	public class Bar
	{
		[Test]
		public void Foo()
		{

			SimpleListenerExample(new[] {"http://localhost:8080/"});
		}



		public static void SimpleListenerExample(string[] prefixes)
		{
			if (!HttpListener.IsSupported)
			{
				Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
				return;
			}


			// URI prefixes are required, 
			// for example "http://contoso.com:8080/index/".
			if (prefixes == null || prefixes.Length == 0)
				throw new ArgumentException("prefixes");
			// Create a listener.
			HttpListener listener = new HttpListener();
			// Add the prefixes. 
			foreach (string s in prefixes)
			{
				listener.Prefixes.Add(s);
			}
			listener.Start();
			Console.WriteLine("Listening...");
			
			HandleRequests(listener);
			
			listener.Stop();
		}

		private static async void HandleRequests(HttpListener listener)
		{
			// Note: The GetContext method blocks while waiting for a request. 
			Console.WriteLine("Waiting for a request");
			HttpListenerContext context = await listener.GetContextAsync();
			HttpListenerRequest request = context.Request;
			// Obtain a response object.
			HttpListenerResponse response = context.Response;
			// Construct a response. 
			string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer, 0, buffer.Length);
			// You must close the output stream.
			output.Close();
		}
	}
}