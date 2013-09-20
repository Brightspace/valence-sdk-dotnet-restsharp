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

		private static readonly List<string> AUTH_TOKENS = new List<string> {
																		"x_a=aaaa" ,
																		"x_b=bbbb" ,
																		"x_c=cccc" ,
																		"x_d=dddd" ,
																		"x_t=tttt" 
																	};

		[SetUp]
		public void Init() {

			var mockUserContext = new Mock<ID2LUserContext>();

			string authQueryParameters = string.Join( "&", AUTH_TOKENS );

			mockUserContext.Setup(
				ctxt => ctxt.CreateAuthenticatedUri( It.IsAny<string>(), It.IsAny<string>() ) )
				.Returns( 
					( string pathAndQuery, string method ) => {

						bool hasQueryParameters = pathAndQuery.IndexOf( '?' ) != -1;

						string fullUrl = 
							BASE_ROUTE +
							pathAndQuery + 
							( hasQueryParameters ? "&" : "?" ) + 
							authQueryParameters;

						return new Uri( fullUrl );
					}
				);

			m_authenticator = new ValenceAuthenticator( mockUserContext.Object );
		}

		private string GenerateExepctedUrl( string pathAndQuery ) {

			bool hasQueryParameters = pathAndQuery.IndexOf( '?' ) != -1;

			string authQueryParameters = string.Join( "&", AUTH_TOKENS );

			string fullPath = 
				pathAndQuery + 
				( hasQueryParameters ? "&" : "?" ) + 
				authQueryParameters;

			return fullPath;
		}

		[Test]
		public void Authenticate_NoQueryParameters_ShouldSucceed(
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
