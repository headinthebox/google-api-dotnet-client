﻿/*
Copyright 2011 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Net;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using NUnit.Framework;

namespace Google.Apis.Authentication.OAuth2.Tests
{
    /// <summary>
    /// Tests for the OAuth2Authenticator class
    /// </summary>
    [TestFixture]
    public class OAuth2AuthenticatorTest
    {
        /// <summary>
        /// Tests the constructor of this class
        /// </summary>
        [Test]
        public void ConstructTest()
        {
            var client = new NativeApplicationClient(new Uri("http://google.com"));
            var auth = new OAuth2Authenticator<NativeApplicationClient>(client, (clt) => new AuthorizationState());
            Assert.IsNotNull(auth.State);
        }

        /// <summary>
        /// Tests that the authorization delegate of this class is called.
        /// </summary>
        [Test]
        public void DelegateTest()
        {
            var state = new AuthorizationState() { AccessToken = "Test" };
            var client = new NativeApplicationClient(new Uri("http://google.com"));
            var auth = new OAuth2Authenticator<NativeApplicationClient>(client, (clt) => state);

            // Check that the state was set.
            Assert.AreEqual(state, auth.State);
        }

        /// <summary>
        /// Tests that an authorization is performed if no access token is available.
        /// </summary>
        [Test]
        public void CheckForValidAccessTokenTest()
        {
            int i = 0;
            var state = new AuthorizationState();
            var client = new NativeApplicationClient(new Uri("http://google.com"));
            var auth = new OAuth2Authenticator<NativeApplicationClient>(
                client, (clt) =>
                {
                    // Load a "cached" access token.
                    state.AccessToken = (i++).ToString();
                    return state;
                });

            // Check that the state was set.
            Assert.AreEqual(state, auth.State);
            Assert.AreEqual("0", auth.State.AccessToken);

            // Check that it wont be set again.
            auth.CheckForValidAccessToken();
            Assert.AreEqual("0", auth.State.AccessToken);

            // Check that it is set if our state gets invalid.
            state.AccessToken = null;
            auth.CheckForValidAccessToken();
            Assert.AreEqual("1", auth.State.AccessToken);
        }

        /// <summary>
        /// Tests that the authorization header is added to a web request.
        /// </summary>
        [Test]
        public void ApplyAuthenticationToRequestTest()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://google.com");
            var state = new AuthorizationState() { AccessToken = "Test" };
            var client = new NativeApplicationClient(new Uri("http://google.com"));
            var auth = new OAuth2Authenticator<NativeApplicationClient>(client, (clt) => state);

            // Confirm that the request header gets modified.
            auth.ApplyAuthenticationToRequest(request);
            Assert.AreEqual(1, request.Headers.Count);
            Assert.AreEqual("OAuth Test", request.Headers["Authorization"]);
        }
    }
}
