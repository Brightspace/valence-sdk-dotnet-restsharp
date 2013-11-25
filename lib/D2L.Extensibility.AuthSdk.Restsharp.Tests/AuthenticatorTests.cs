using System;
using System.Collections.Generic;
using System.Linq;

using Moq;

using NUnit.Framework;

using RestSharp;

namespace D2L.Extensibility.AuthSdk.Restsharp.Tests {

	[TestFixture]
	public class AuthenticatorTests {

		private ValenceAuthenticator m_authenticator;

		const string BASE_ROUTE = "http://someserver.com:44444";

		private static readonly List<Tuple<string,string>> AUTH_TOKENS = new List<Tuple<string, string>> {
																		new Tuple<string, string>("x_a","aaaa"),
																		new Tuple<string, string>("x_b","bbbb"),
																		new Tuple<string, string>("x_c","cccc"),
																		new Tuple<string, string>("x_d","dddd"),
																		new Tuple<string, string>("x_t","tttt"),
																	};

		[SetUp]
		public void Init() {

			var mockUserContext = new Mock<ID2LUserContext>();

			string authQueryParameters = string.Join( "&", AUTH_TOKENS );

			mockUserContext.Setup(
				ctxt => ctxt.CreateAuthenticatedTokens( It.IsAny<Uri>(), It.IsAny<string>() ) )
				.Returns( ( Uri fullUrl, string method ) => AUTH_TOKENS );

			m_authenticator = new ValenceAuthenticator( mockUserContext.Object );
		}

		private string GenerateExepctedUrl( string pathAndQuery ) {

			bool hasQueryParameters = pathAndQuery.IndexOf( '?' ) != -1;

			var tokens = AUTH_TOKENS.Select( token => token.Item1 + "=" + token.Item2 );

			string authQueryParameters = string.Join( "&", tokens );

			string fullPath = 
				pathAndQuery + 
				( hasQueryParameters ? "&" : "?" ) + 
				authQueryParameters;

			return fullPath;
		}

		[Test]
		public void Authenticate_QueryParameters_ShouldSucceed(
				[Values(
					"/api/versions/", 
					"/api/collection/resource/?sort=asc", 
					"/api/versions?sort=asc",
					"/api/versions/?sort=asc&count=10",
					"/api/collection/resource?sort=asc&count=10")]string pathAndQuery
			) {

			var client = new RestClient( BASE_ROUTE );

			var request = new RestRequest( pathAndQuery );

			m_authenticator.Authenticate( client, request );

			string expectedUrl = GenerateExepctedUrl( pathAndQuery );

			Assert.AreEqual( expectedUrl, request.Resource );
		}
	}
}
