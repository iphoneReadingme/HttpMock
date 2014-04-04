using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;

namespace HttpMock
{
	public class EndpointMatchingRule : IMatchingRule
	{
		public bool IsEndpointMatch(IRequestHandler requestHandler, HttpRequestHead request)
		{
			if (requestHandler.QueryParams == null)
				throw new ArgumentException("requestHandler QueryParams cannot be null");

			var requestQueryParams = GetQueryParams(request);

			bool uriStartsWith = request.Uri.StartsWith(requestHandler.Path);

			bool httpMethodsMatch = requestHandler.Method == request.Method;

			bool queryParamMatch = true;
			bool shouldMatchQueryParams = (requestHandler.QueryParams.Count > 0);

			if (shouldMatchQueryParams)
			{
				queryParamMatch = new QueryParamMatch().MatchQueryParams(requestHandler, requestQueryParams);
			}

			return uriStartsWith && httpMethodsMatch && queryParamMatch;
		}

		private static IDictionary<string, string> GetQueryParams(HttpRequestHead request)
		{
			return request.QueryString;
		}
	}
}