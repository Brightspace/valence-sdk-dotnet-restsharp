using System;
using System.Collections.Generic;

using RestSharp;

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

		private IEnumerable<Tuple<string,string>> CreateAuthenticationTokens( 
				string pathAndQuery, 
				string method 
			) {

			var fullUri = m_context.CreateAuthenticatedUri( pathAndQuery, method );

			var baseAndQuery = fullUri.Query.Split( '?' );

			if( baseAndQuery.Length != 2 ) {
				yield break;
			}

			var queryParameters = baseAndQuery[1].Split( '&' );

			foreach( var parameter in queryParameters ) {

				var tokens = parameter.Split( '=' );
				var key = tokens[0];
				var value = tokens[1];

				if( key.StartsWith( "x_a" ) || 
					key.StartsWith( "x_b" ) || 
					key.StartsWith( "x_c" ) || 
					key.StartsWith( "x_d" ) || 
					key.StartsWith( "x_t" ) ) {

					yield return new Tuple<string, string>( key, value );
				}
			}
		}

		private string CreateAuthQueryString( IEnumerable<Tuple<string, string>> tokens  ) {

			var queryParametersList = new List<string>();

			foreach( var token in tokens ) {
				queryParametersList.Add( string.Format( "{0}={1}", token.Item1, token.Item2 ) );
			}

			var authQueryParameters = string.Join( "&", queryParametersList );

			return authQueryParameters;
		}

		public void Authenticate( IRestClient client, IRestRequest request ) {

			var uri = client.BuildUri( request );

			string method = AdaptMethod( request.Method );

			var tokens = CreateAuthenticationTokens( uri.PathAndQuery, method );

			var authQueryParameters = CreateAuthQueryString( tokens );

			var url = uri.ToString();

			// manually set the resource url to work around RestSharp not letting you add query parameters 
			// once you've added a body to the HTTP request
			bool hasQueryParameters = url.IndexOf( '?' ) != -1;

			if( hasQueryParameters ) {

				var resource = uri.PathAndQuery;

				var index = resource.IndexOf( request.Resource, System.StringComparison.Ordinal );

				// need to trim starting resource
				request.Resource = resource.Substring( index, resource.Length - index );
				request.Resource += "&" + authQueryParameters;
				request.Parameters.Clear();
			} else {
				request.Resource += "?" + authQueryParameters;
			}
		}
	}
}
