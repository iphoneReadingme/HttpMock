using System;
using System.Collections.Generic;

namespace HttpMock
{
	public class QueryParamMatch {
	    internal bool MatchQueryParams(IRequestHandler requestHandler, IDictionary<string, string> requestQueryParams) {
			foreach (var queryParam in requestHandler.QueryParams) {
				if (!requestQueryParams.ContainsKey(queryParam.Key)) {
					return false;
				}
				if (!String.Equals(requestQueryParams[queryParam.Key], queryParam.Value, StringComparison.OrdinalIgnoreCase)) {
					return false;
				}
			}
			return true;
		}
	}
}