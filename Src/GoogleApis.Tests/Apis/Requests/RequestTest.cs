/*
Copyright 2010 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Google.Apis.Authentication;
using NUnit.Framework;
using Google.Apis.Requests;
using Google.Apis.Discovery;

namespace Google.Apis.Tests.Apis.Requests
{
    /// <summary>
    /// Tests for the "Request" class
    /// </summary>
    [TestFixture]
    public class RequestTest
    {
        private const string SimpleDeveloperKey = "ABC123";
        private const string ComplexDeveloperKey = "?&^%  ABC123";

        /// <summary>
        /// Tests a request with default settings
        /// </summary>
        [Test]
        public void BuildRequestUrlWithDefaultedParameters()
        {
            var parameterDefinitions = new Dictionary<string, IParameter>();
            parameterDefinitions.Add(
                "required", new MockParameter { Name = "required", IsRequired = true, ParameterType = "query" });
            parameterDefinitions.Add(
                "optionalWithValue",
                new MockParameter
                    {
                        Name = "optionalWithValue",
                        IsRequired = false,
                        ParameterType = "query",
                        DefaultValue = "DoesNotDisplay"
                    });
            parameterDefinitions.Add(
                "optionalWithNull",
                new MockParameter
                    { Name = "optionalWithNull", IsRequired = false, ParameterType = "query", DefaultValue = "c" });
            parameterDefinitions.Add(
                "optionalWithEmpty",
                new MockParameter
                    { Name = "optionalWithEmpty", IsRequired = false, ParameterType = "query", DefaultValue = "d" });
            parameterDefinitions.Add(
                "optionalNotPressent",
                new MockParameter
                    {
                        Name = "optionalNotPressent",
                        IsRequired = false,
                        ParameterType = "query",
                        DefaultValue = "DoesNotDisplay"
                    });
            var parameterValues = new SortedDictionary<string, string>();
            parameterValues.Add("required", "a");
            parameterValues.Add("optionalWithValue", "b");
            parameterValues.Add("optionalWithNull", null);
            parameterValues.Add("optionalWithEmpty", "");

            var service = new MockService();
            var request =
                (Request)
                Request.CreateRequest(
                    service,
                    new MockMethod
                        {
                            HttpMethod = "GET",
                            Name = "TestMethod",
                            RestPath = "https://example.com",
                            Parameters = parameterDefinitions
                        });

            request.WithParameters(parameterValues);
            var url = request.BuildRequestUrl();

            Assert.AreEqual(
                "https://example.com/?alt=json&optionalWithEmpty=d&" +
                "optionalWithNull=c&optionalWithValue=b&required=a", url.AbsoluteUri);
        }

        /// <summary>
        /// Builds a request from a query string, and checks if the resulting query string is the same.
        /// </summary>
        [Test]
        public void BuildRequestWithQueryStringTest()
        {
            const string query = "required=yes&optionalWithValue=%26test";

            var parameterDefinitions = new Dictionary<string, IParameter>();
            parameterDefinitions.Add(
                "required", new MockParameter { Name = "required", IsRequired = true, ParameterType = "query" });
            parameterDefinitions.Add(
                "optionalWithValue",
                new MockParameter { Name = "optionalWithValue", IsRequired = false, ParameterType = "query" });
            var service = new MockService();

            // Cast the IRequest to a Request to access internal construction methods.
            var request =
                (Request)
                Request.CreateRequest(
                    service,
                    new MockMethod
                    {
                        HttpMethod = "GET",
                        Name = "TestMethod",
                        RestPath = "https://test.google.com",
                        Parameters = parameterDefinitions
                    });

            request.WithParameters(query);
            var url = request.BuildRequestUrl();

            // Check that the resulting query string is identical with the input.
            Assert.AreEqual("?alt=json&"+query, url.Query);
        }

        /// <summary>
        /// Tests if the WithDeveloperKey method works
        /// </summary>
        [Test]
        public void BuildRequestUrlWithDeveloperKeysTest()
        {
            var service = new MockService();
            var request =
                (Request)
                Request.CreateRequest(
                    service,
                    new MockMethod { HttpMethod = "GET", Name = "TestMethod", RestPath = "https://example.com" });
            request.WithDeveloperKey(SimpleDeveloperKey).WithParameters(new Dictionary<string, string>());
            var url = request.BuildRequestUrl();

            Assert.AreEqual("https://example.com/?alt=json" + "&key=" + SimpleDeveloperKey, url.ToString());
        }

        /// <summary>
        /// Tests if developer keys are escaped properly
        /// </summary>
        [Test]
        public void BuildRequestUrlWithDeveloperKeysTest_RequiresEscape()
        {
            var service = new MockService();
            var request =
                (Request)
                Request.CreateRequest(
                    service,
                    new MockMethod { HttpMethod = "GET", Name = "TestMethod", RestPath = "https://example.com" });
            request.WithDeveloperKey(ComplexDeveloperKey).WithParameters(new Dictionary<string, string>());
            var url = request.BuildRequestUrl();

            Assert.AreEqual("https://example.com/?alt=json" + "&key=%3F%26%5E%25%20%20ABC123", url.AbsoluteUri);
        }

        /// <summary>
        /// Tests if null is handled correctly @ optional parameters
        /// </summary>
        [Test]
        public void BuildRequestUrlWithNullOptionalParameters()
        {
            var parameterDefinitions = new Dictionary<string, IParameter>();
            parameterDefinitions.Add(
                "required", new MockParameter { Name = "required", IsRequired = true, ParameterType = "query" });
            parameterDefinitions.Add(
                "optionalWithValue",
                new MockParameter { Name = "optionalWithValue", IsRequired = false, ParameterType = "query" });
            parameterDefinitions.Add(
                "optionalWithNull",
                new MockParameter { Name = "optionalWithNull", IsRequired = false, ParameterType = "query" });
            parameterDefinitions.Add(
                "optionalWithEmpty",
                new MockParameter { Name = "optionalWithEmpty", IsRequired = false, ParameterType = "query" });
            parameterDefinitions.Add(
                "optionalNotPressent",
                new MockParameter { Name = "optionalNotPressent", IsRequired = false, ParameterType = "query" });
            var parameterValues = new SortedDictionary<string, string>();
            parameterValues.Add("required", "a");
            parameterValues.Add("optionalWithValue", "b");
            parameterValues.Add("optionalWithNull", null);
            parameterValues.Add("optionalWithEmpty", "");

            var service = new MockService();
            var request =
                (Request)
                Request.CreateRequest(
                    service,
                    new MockMethod
                        {
                            HttpMethod = "GET",
                            Name = "TestMethod",
                            RestPath = "https://example.com",
                            Parameters = parameterDefinitions
                        });

            request.WithParameters(parameterValues);
            var url = request.BuildRequestUrl();

            Assert.AreEqual("https://example.com/?alt=json&optionalWithValue=b&required=a", url.AbsoluteUri);
        }

        /// <summary>
        /// Tests if the constructor will succeed
        /// </summary>
        [Test]
        public void ConstructorTest()
        {
            var request = new Request();
            Assert.IsNotNull(request.Authenticator);
            Assert.IsTrue(request.SupportsRetry);
        }

        /// <summary>
        /// Tests the application name property
        /// </summary>
        [Test]
        public void FormatForUserAgentTest()
        {
            var request = new Request();

            Assert.AreEqual("Unknown_Application", request.FormatForUserAgent("Unknown Application"));
            Assert.AreEqual("T_e_s_t", request.FormatForUserAgent("T e s t"));
        }

        /// <summary>
        /// Tests if invalid data will throw an exception
        /// </summary>
        [Test]
        public void CreateRequestFailureTest()
        {
            var service = new MockService();
            Assert.Throws<NotSupportedException>(
                () =>
                Request.CreateRequest(
                    service,
                    new MockMethod
                        { HttpMethod = "NOSUCHMETHOD", Name = "TestMethod", RestPath = "https://example.com", }));
        }

        /// <summary>
        /// Tests the creation of simple http requests
        /// </summary>
        [Test]
        public void CreateRequestSimpleCreateTest()
        {
            var service = new MockService();
            Request.CreateRequest(
                service,
                new MockMethod { HttpMethod = "GET", Name = "TestMethod", RestPath = "https://example.com", });
            Request.CreateRequest(
                service,
                new MockMethod { HttpMethod = "POST", Name = "TestMethod", RestPath = "https://example.com", });
            Request.CreateRequest(
                service,
                new MockMethod { HttpMethod = "PUT", Name = "TestMethod", RestPath = "https://example.com", });
            Request.CreateRequest(
                service,
                new MockMethod { HttpMethod = "DELETE", Name = "TestMethod", RestPath = "https://example.com", });
            Request.CreateRequest(
                service,
                new MockMethod { HttpMethod = "PATCH", Name = "TestMethod", RestPath = "https://example.com", });
        }

        /// <summary>
        /// Tests the user-agent string of the request created by the .CreateRequest method.
        /// </summary>
        [Test]
        public void CreateRequestOnRequestUserAgentTest()
        {
            var service = new MockService();
            var request = (Request)Request.CreateRequest(
                service,
                new MockMethod
                {
                    HttpMethod = "GET",
                    Name = "TestMethod",
                    RestPath = "https://example.com/test",
                });

            request.WithParameters("");

            HttpWebRequest webRequest = (HttpWebRequest)request.CreateWebRequest();

            // Test the default user agent (without gzip):
            string expectedUserAgent = string.Format(
                "Unknown_Application google-api-dotnet-client/{0} {1}/{2}", Utilities.GetAssemblyVersion(),
                Environment.OSVersion.Platform, Environment.OSVersion.Version);
            Assert.AreEqual(expectedUserAgent, webRequest.UserAgent);

            // Confirm that the (gzip) tag is added if GZip is supported.
            service.GZipEnabled = true;
            expectedUserAgent += " (gzip)";
            webRequest = (HttpWebRequest)request.CreateWebRequest();
            Assert.AreEqual(expectedUserAgent, webRequest.UserAgent);
        }

        /// <summary>
        /// Confirms that the .CreateRequest method sets the Content-Length of requests declaring a body.
        /// </summary>
        [Test]
        public void CreateRequestOnRequestContentLengthTest()
        {
            var service = new MockService();
            var request = (Request)Request.CreateRequest(
                service,
                new MockMethod
                {
                    HttpMethod = "POST",
                    Name = "TestMethod",
                    RestPath = "https://example.com/test",
                });

            request.WithParameters("");
            HttpWebRequest webRequest = (HttpWebRequest)request.CreateWebRequest();

            // Test that the header is set, even if no body is specified.
            Assert.AreEqual(0, webRequest.ContentLength);

            // The content-length field will be automatically set as soon as content is written into the
            // request stream. We cannot test it here as writing to the request stream requires us to have
            // a valid connection to the server.
        }

        /// <summary>
        /// Tests the .CreateRequest method of a request
        /// </summary>
        [Test]
        public void CreateRequestOnRequestTest()
        {
            var service = new MockService();
            var request = (Request)Request.CreateRequest(
                service,
                new MockMethod { HttpMethod = "GET", Name = "TestMethod", RestPath = "https://example.com",
                Parameters = new Dictionary<string, IParameter>() { {"TestParam", null} }});
            
            request.WithParameters("");

            HttpWebRequest webRequest = (HttpWebRequest)request.CreateWebRequest();
            Assert.IsNotNull(webRequest);
        }

        /// <summary>
        /// Tests if the developer key is assigned correctly
        /// </summary>
        [Test]
        public void WithDeveloperKeyAssignTest()
        {
            var request =
                (Request)
                Request.CreateRequest(
                    new MockService(),
                    new MockMethod { HttpMethod = "GET", Name = "TestMethod", RestPath = "https://example.com" });
            request.WithDeveloperKey(SimpleDeveloperKey);
            Assert.AreEqual(SimpleDeveloperKey, request.DeveloperKey);
        }

        /// <summary>
        /// Tests if the parameter dictionary is set correctly
        /// </summary>
        [Test]
        public void WithParametersDictStringObject()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("a", "a");
            dict.Add("null", null);
            dict.Add("1", 1);
            dict.Add("100", 100L);
            dict.Add("True", true);
            dict.Add("False", false);
            var request = new Request();
            request.WithParameters(dict);
            Assert.AreEqual(dict.Count, request.Parameters.Count);
            Assert.AreEqual("a", request.Parameters.GetFirstMatch("a"));
            Assert.IsNull(request.Parameters.GetFirstMatch("null"));
            Assert.AreEqual("1", request.Parameters.GetFirstMatch("1"));
            Assert.AreEqual("100", request.Parameters.GetFirstMatch("100"));
            Assert.AreEqual("True", request.Parameters.GetFirstMatch("True"));
            Assert.AreEqual("False",request.Parameters.GetFirstMatch("False"));
        }

        private class MockAuthenticator : Authenticator {}

        private class MockErrorHandlingAuthenticator : Authenticator, IErrorResponseHandler
        {
            public bool CanHandleErrorResponse(WebException exception, RequestError error)
            {
                return false;
            }

            public void PrepareHandleErrorResponse(WebException exception, RequestError error) {}

            public void HandleErrorResponse(WebException exception, RequestError error, WebRequest request) {}
        }

        /// <summary>
        /// Tests the results of the GetErrorResponseHandlers method.
        /// </summary>
        [Test]
        public void GetErrorResponseHandlersTest()
        {
            var request =
              (Request)
              Request.CreateRequest(
                  new MockService(),
                  new MockMethod { HttpMethod = "GET", Name = "TestMethod", RestPath = "https://example.com" });

            // Confirm that there are no error response handlers by default.
            CollectionAssert.IsEmpty(request.GetErrorResponseHandlers());

            // Confirm that a standard authenticator won't result in an error response handler.
            request.WithAuthentication(new MockAuthenticator());
            CollectionAssert.IsEmpty(request.GetErrorResponseHandlers());

            // Confirm that an error handling response handler will change the enumeration
            var auth = new MockErrorHandlingAuthenticator();
            request.WithAuthentication(auth);
            CollectionAssert.AreEqual(new IErrorResponseHandler[] { auth }, request.GetErrorResponseHandlers());
        }
    }
}