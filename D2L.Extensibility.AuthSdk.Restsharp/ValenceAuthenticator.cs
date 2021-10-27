using System;
using System.Collections.Generic;
using System.Linq;

using RestSharp;
using RestSharp.Authenticators;

namespace D2L.Extensibility.AuthSdk.Restsharp {

	public sealed class ValenceAuthenticator : IAuthenticator {

		private readonly ID2LUserContext m_context;

		public ValenceAuthenticator( ID2LUserContext context ) {
			m_context = context;
		}

		private string AdaptMethod( Method m ) {

			string method;

			switch( m ) {
				case Method.DELETE:
					method = "DELETE";
					break;
				case Method.GET:
					method = "GET";
					break;
				case Method.POST:
					method = "POST";
					break;
				case Method.PUT:
					method = "PUT";
					break;
				default:
					throw new ArgumentException( "Unhandled method: " + m );
			}

			return method;
		}

		private string CreateAuthQueryString( IEnumerable<Tuple<string, string>> tokens  ) {

			var queryParametersList = 
				tokens.Select( token => string.Format( "{0}={1}", token.Item1, token.Item2 ) );

			var authQueryParameters = string.Join( "&", queryParametersList );

			return authQueryParameters;
		}

		public void Authenticate( IRestClient client, IRestRequest request ) {

			var uri = client.BuildUri( request );

			string method = AdaptMethod( request.Method );

			var tokens = m_context.CreateAuthenticatedTokens(uri, method);

			var authQueryParameters = CreateAuthQueryString( tokens );

			string url = uri.ToString();

			// manually set the resource url to work around RestSharp not letting you add query parameters 
			// once you've added a body to the HTTP request
			bool hasQueryParameters = url.IndexOf( '?' ) != -1;

			if( hasQueryParameters ) {
				request.Resource = uri.PathAndQuery;
				request.Resource += "&" + authQueryParameters;
				request.Parameters.Clear();
			} else {
				request.Resource += "?" + authQueryParameters;
			}
		}
	}
}
