using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

using log4net;

namespace HttpMock
{
	public interface IRequestProcessor
	{
		RequestHandler FindHandler(string method, string path);
	}

	public class RequestProcessor :  IRequestProcessor
	{
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IMatchingRule _matchingRule;
		private RequestHandlerList _handlers;

		public RequestProcessor(IMatchingRule matchingRule, RequestHandlerList requestHandlers)
		{
			_matchingRule = matchingRule;
			_handlers = requestHandlers;
		}


		private static void HandleRequest(HttpListenerResponse response,
			RequestHandler handler, HttpRequestHead requestHead)
		{
			_log.DebugFormat("Matched a handler {0},{1}, {2}", handler.Method, handler.Path, DumpQueryParams(handler.QueryParams));

			var respondWith  = GetDataProducer(requestHead, handler);
			if (requestHead.HasEntityBody)
			{
				handler.RecordRequest(requestHead, requestHead.Body);
				_log.DebugFormat("Body: {0}", requestHead.Body);
			}
			else
			{
				handler.RecordRequest(requestHead, null);
			}

			HttpResponseHead httpResponseHead = handler.ResponseBuilder.BuildHeaders();
			
			foreach (var header in httpResponseHead.Headers)
			{
				response.AppendHeader(header.Key, header.Value);
			}
			response.ContentType = httpResponseHead.ContentType;
			response.ContentLength64 = httpResponseHead.ContentLength;

			respondWith.Connect(response.OutputStream);
			_log.DebugFormat("End Processing request for : {0}:{1}", requestHead.Method, requestHead.Uri);
			response.Close();
		}

		private static IResponse GetDataProducer(HttpRequestHead request, RequestHandler handler)
		{
			return request.Method != "HEAD"
				? handler.ResponseBuilder.BuildBody(request.Headers)
				: null;
		}

		private int GetHandlerCount()
		{
			return _handlers.Count();
		}

		private RequestHandler MatchHandler(HttpRequestHead request)
		{
			var matches = _handlers
				.Where(handler => _matchingRule.IsEndpointMatch(handler, request))
				.Where(handler => handler.CanVerifyConstraintsFor(request.Uri.ToString()));

			return matches.FirstOrDefault();
		}

		public RequestHandler FindHandler(string method, string path)
		{
			return _handlers.Where(x => x.Path == path && x.Method == method).FirstOrDefault();
		}

		private static string DumpQueryParams(IDictionary<string, string> queryParams)
		{
			var sb = new StringBuilder();
			foreach (var param in queryParams)
			{
				sb.AppendFormat("{0}={1}&", param.Key, param.Value);
			}
			return sb.ToString();
		}

		private static void ReturnHttpMockNotFound(HttpListenerResponse response)
		{
			
			response.StatusCode = (int) HttpStatusCode.NotFound;
			response.StatusDescription = string.Format("{0} {1}", 404, "NotFound");
			response.ContentLength64 = 0;
			response.AppendHeader("X-HttpMockError", "No handler found to handle request");

		}

		public void ClearHandlers()
		{
			_handlers = new RequestHandlerList();
		}

		public void Add(RequestHandler requestHandler)
		{
			_handlers.Add(requestHandler);
		}

		public string WhatDoIHave()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Handlers:");
			foreach (RequestHandler handler in _handlers)
			{
				stringBuilder.Append(handler.ToString());
			}
			return stringBuilder.ToString();
		}


		public void Process(HttpListenerRequest request, HttpListenerResponse response)
		{
			_log.DebugFormat("Start Processing request for : {0}:{1}", request.HttpMethod, request.Url);
			if (GetHandlerCount() < 1)
			{
				ReturnHttpMockNotFound(response);
				return;
			}

			HttpRequestHead httpRequestHead = new HttpRequestHead
			{
				Method = request.HttpMethod,
				Headers = request.Headers.AllKeys.ToDictionary( key => key, key => request.Headers.Get(key)),
				Body = new StreamReader(request.InputStream).ReadToEnd(),
				QueryString = request.QueryString.AllKeys.ToDictionary(key => key, key => request.QueryString.Get(key)),
				HasEntityBody = request.HasEntityBody,
				Uri = request.RawUrl

			};
			RequestHandler handler = MatchHandler(httpRequestHead);

			if (handler == null)
			{
				_log.DebugFormat("No Handlers matched");
				ReturnHttpMockNotFound(response);
				return;
			}
			HandleRequest(response, handler, httpRequestHead);
		}
	}
}