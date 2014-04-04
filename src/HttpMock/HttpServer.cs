using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using log4net;

namespace HttpMock
{
	public class HttpServer : IHttpServer
	{
		private readonly RequestProcessor _requestProcessor;
		private readonly RequestWasCalled _requestWasCalled;
		private readonly RequestWasNotCalled _requestWasNotCalled;
		private readonly Uri _uri;
		private IDisposable _disposableServer;
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private Thread _thread;
		private readonly RequestHandlerFactory _requestHandlerFactory;
		private HttpListener _httpListener;

		public HttpServer(Uri uri) {
			_uri = uri;
			_requestProcessor = new RequestProcessor(new EndpointMatchingRule(), new RequestHandlerList());
			_requestWasCalled = new RequestWasCalled(_requestProcessor);
			_requestWasNotCalled = new RequestWasNotCalled(_requestProcessor);
			_requestHandlerFactory = new RequestHandlerFactory(_requestProcessor);
			_httpListener = new HttpListener();
		}

		public void Start() {

			_httpListener.Prefixes.Clear();
			_httpListener.Prefixes.Add(_uri.ToString());
			_httpListener.Start();

			if (!IsAvailable())
			{
				throw new InvalidOperationException("Kayak server not listening yet.");
			}

			IAsyncResult asyncResult = _httpListener.BeginGetContext(AcceptRequest, _httpListener);
			


			
			
		}

		private void AcceptRequest(IAsyncResult ar)
		{
			var listener = (HttpListener)ar.AsyncState;
			var context = listener.EndGetContext(ar);

			listener.BeginGetContext(AcceptRequest, listener);

			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			_requestProcessor.Process(request, response);


		}

		public bool IsAvailable()
		{
			return _httpListener.IsListening;
		}

		public void Dispose()
		{
			if (_httpListener != null)
			{
				_httpListener.Stop();
			}
		}

		public RequestHandler Stub(Func<RequestHandlerFactory, RequestHandler> func) {
			return func.Invoke(_requestHandlerFactory);
		}

		public RequestHandler AssertWasCalled(Func<RequestWasCalled, RequestHandler> func) {
			return func.Invoke(_requestWasCalled);
		}

		public RequestHandler AssertWasNotCalled(Func<RequestWasNotCalled, RequestHandler> func) {
			return func.Invoke(_requestWasNotCalled);
		}

		public IHttpServer WithNewContext() {
			_requestProcessor.ClearHandlers();
			return this;
		}

		public IHttpServer WithNewContext(string baseUri) {
			WithNewContext();
			return this;
		}

		public string WhatDoIHave() {
			return _requestProcessor.WhatDoIHave();
		}
	}
}