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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using Google.Apis.Authentication;
using Google.Apis.Discovery;
using Google.Apis.Testing;
using Google.Apis.Util;

namespace Google.Apis.Requests
{
    /// <summary>
    /// Request to a service
    /// </summary>
    public class Request : IRequest
    {
        private const string UserAgent = "{0} google-api-dotnet-client/{1}";
        private const string GZipUserAgentSuffix = " (gzip)";
        private const string GZipEncoding = "gzip";

        private static readonly String ApiVersion = typeof(Request).Assembly.GetName().Version.ToString();
        private Uri requestUrl;

        public Request()
        {
            AppName = "Unknown Application";
            Authenticator = new NullAuthenticator();
        }

        /// <summary>
        /// The authenticator used for this request
        /// </summary>
        internal IAuthenticator Authenticator { get; private set; }

        /// <summary>
        /// The name of the application making the request. Affects the User Agent of this client. 
        /// </summary>
        internal String AppName { get; private set; }

        /// <summary>
        /// The developer API Key sent along with the request
        /// </summary>
        internal String DeveloperKey { get; private set; }

        /// <summary>
        /// Set of method parameters
        /// </summary>
        [VisibleForTestOnly]
        internal IDictionary<string, string> Parameters { get; set; }

        private IService Service { get; set; }
        private IMethod Method { get; set; }
        private Uri BaseURI { get; set; }
        private string RPCName { get; set; } // note: this property is apparently never used
        private string Body { get; set; }
        private ReturnType ReturnType { get; set; }

        #region IRequest Members

        /// <summary>
        /// The method to call
        /// </summary>
        /// <returns>
        /// A <see cref="Request"/>
        /// </returns>
        public IRequest On(string rpcName)
        {
            RPCName = rpcName;

            return this;
        }

        /// <summary>
        /// Sets the type of data that is expected to be returned from the request.
        /// 
        /// Defaults to Json
        /// </summary>
        /// <param name="returnType">
        /// A <see cref="ReturnType"/>
        /// </param>
        /// <returns>
        /// A <see cref="Request"/>
        /// </returns>
        public IRequest Returning(ReturnType returnType)
        {
            ReturnType = returnType;
            return this;
        }


        /// <summary>
        /// Adds the parameters to the request.
        /// </summary>
        /// <returns>
        /// A <see cref="Request"/>
        /// </returns>
        public IRequest WithParameters(IDictionary<string, object> parameters)
        {
            return WithParameters(parameters.ToDictionary(k => k.Key, v => v.Value != null ? v.Value.ToString() : null));
        }


        /// <summary>
        /// Adds the parameters to the request.
        /// </summary>
        /// <returns>
        /// A <see cref="Request"/>
        /// </returns>
        public IRequest WithParameters(IDictionary<string, string> parameters)
        {
            Parameters = parameters;
            return this;
        }

        /// <summary>
        /// Adds a set of URL encoded parameters to the request
        /// </summary>
        public IRequest WithParameters(string parameters)
        {
            Parameters = Utilities.QueryStringToDictionary(parameters);
            return this;
        }

        /// <summary>
        /// Uses the string provied as the body of the request.
        /// </summary>
        public IRequest WithBody(string body)
        {
            Body = body;
            return this;
        }

        /// <summary>
        /// Uses the provided authenticator to add authentication information to this request.
        /// </summary>
        public IRequest WithAuthentication(IAuthenticator authenticator)
        {
            authenticator.ThrowIfNull("Authenticator");
            Authenticator = authenticator;
            return this;
        }

        /// <summary>
        /// Adds the developer key to this request
        /// </summary>
        public IRequest WithDeveloperKey(string key)
        {
            DeveloperKey = key;
            return this;
        }

        /// <summary>
        /// Executes a request given the configuration options supplied.
        /// </summary>
        /// <returns>
        /// A <see cref="Stream"/>
        /// </returns>
        public Stream ExecuteRequest()
        {
            // Validate the input
            var validator = new MethodValidator(Method, Parameters);
            if (validator.ValidateAllParameters() == false)
            {
                return Stream.Null;
            }

            // Create the request
            HttpWebRequest request = CreateRequest();

            // Attach a body if a POST and there is something to attach.
            if (String.IsNullOrEmpty(Body) == false && (Method.HttpMethod == "POST" || Method.HttpMethod == "PUT"))
            {
                AttachBody(request);
            }

            // Execute the request
            try
            {
                var response = (HttpWebResponse) request.GetResponse();
                return response.GetResponseStream();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return ex.Response.GetResponseStream();
                }

                // The exception is not something the client can handle via a stream.
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Given an API method, create the appropriate Request for it.
        /// </summary>
        public static Request CreateRequest(IService service, IMethod method)
        {
            switch (method.HttpMethod)
            {
                case "GET":
                    return new GETRequest { Service = service, Method = method, BaseURI = service.BaseUri };
                case "PUT":
                    return new PUTRequest { Service = service, Method = method, BaseURI = service.BaseUri };
                case "POST":
                    return new POSTRequest { Service = service, Method = method, BaseURI = service.BaseUri };
                case "DELETE":
                    return new DELETERequest { Service = service, Method = method, BaseURI = service.BaseUri };
            }

            throw new ArgumentOutOfRangeException(
                "method", string.Format("Unknown HTTP method \"{0}\"", method.HttpMethod));
        }

        /// <summary>
        /// Sets the Application name on the UserAgent String
        /// </summary>
        /// <param name="name">
        /// A <see cref="System.String"/>
        /// </param>
        public IRequest WithAppName(string name)
        {
            AppName = name;
            return this;
        }

        /// <summary>
        /// Builds the resulting Url for the whole request
        /// </summary>
        [VisibleForTestOnly]
        internal Uri BuildRequestUrl()
        {
            string restPath = Method.RestPath;
            var queryParams = new List<string>();

            queryParams.Add(ReturnType == ReturnType.Json ? "alt=json" : "alt=atom");

            if (DeveloperKey.IsNotNullOrEmpty())
            {
                queryParams.Add(
                    "key=" + Uri.EscapeUriString(DeveloperKey). // Escapses most of what we need
                                 Replace("&", "%26"). // Also escaped & and ?
                                 Replace("?", "%3F"));
            }

            // Replace the substitution parameters
            foreach (var parameter in Parameters)
            {
                IParameter parameterDefinition = Method.Parameters[parameter.Key];
                string value = parameter.Value;
                if (value.IsNullOrEmpty()) // If the parameter is present and has no value, use the default
                {
                    value = parameterDefinition.DefaultValue;
                }
                switch (parameterDefinition.ParameterType)
                {
                    case "path":
                        restPath = restPath.Replace(String.Format("{{{0}}}", parameter.Key), value);
                        break;
                    case "query":
                        // If the parameter is optional and no value is given, don't add to url.
                        if (parameterDefinition.Required == false && value.IsNullOrEmpty())
                        {
                            continue;
                        }
                        queryParams.Add(parameterDefinition.Name + "=" + value);
                        break;
                    default:
                        throw new NotSupportedException(
                            "Found an unsupported Parametertype [" + parameterDefinition.ParameterType + "]");
                }
            }

            // URL encode the parameters and append them to the URI
            string path = restPath;
            if (queryParams.Count > 0)
            {
                path += "?" + String.Join("&", queryParams.ToArray());
            }


            return new Uri(BaseURI, path);
        }

        private static string GetReturnMimeType(ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.Atom:
                    return "application/atom+xml";
                case ReturnType.Json:
                    return "application/json";
                default:
                    throw new ArgumentOutOfRangeException("returnType", returnType, "Unknown return type");
            }
        }

        private HttpWebRequest CreateRequest()
        {
            // Formulate the RequestUrl
            requestUrl = BuildRequestUrl();

            // Create the request
            HttpWebRequest request = Authenticator.CreateHttpWebRequest(Method.HttpMethod, requestUrl);

            // Insert the content type and user agent
            request.ContentType = GetReturnMimeType(ReturnType);
            request.UserAgent = String.Format(UserAgent, AppName, ApiVersion);

            // Check if compression is supported
            if (Service.GZipEnabled)
            {
                request.UserAgent += GZipUserAgentSuffix;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return request;
        }

        private void AttachBody(HttpWebRequest request)
        {
            Stream bodyStream = request.GetRequestStream();

            // If enabled: Encapsulate in GZipStream
            if (Service.GZipEnabled)
            {
                // Change the content encoding and apply a gzip filter
                request.Headers.Add(HttpRequestHeader.ContentEncoding, GZipEncoding);
                bodyStream = new GZipStream(bodyStream, CompressionMode.Compress);
            }

            // Write data into the stream
            using (bodyStream)
            {
                byte[] postBody = Encoding.ASCII.GetBytes(Body);
                bodyStream.Write(postBody, 0, postBody.Length);
            }
        }
    }
}