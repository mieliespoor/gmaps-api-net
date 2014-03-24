﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Google.Maps.Internal
{
    /// <summary>
    /// Provides an intuitive and simple HTTP client wrapper.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Performs the request and formats the response object from the api request
        /// </summary>
        public class HttpGetResponse
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HttpGetResponse"/> class.
            /// </summary>
            /// <param name="uri">the <see cref="Uri"/> used to make the request.</param>
            public HttpGetResponse(Uri uri)
            {
                RequestUri = uri;
            }

            /// <summary>
            /// Gets or sets the <see cref="Uri"/> used to make the request.
            /// </summary>
            protected Uri RequestUri { get; set; }

            protected async virtual Task<StreamReader> GetStreamReaderAsync(Uri uri)
            {
                return await GetStreamReaderAsync(uri, GoogleSigned.SigningInstance);
            }
            
            protected async virtual Task<StreamReader> GetStreamReaderAsync(Uri uri, GoogleSigned signingInstance)
            {
                if (signingInstance != null)
                {
                    uri = new Uri(signingInstance.GetSignedUri(uri));
                }

                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    return new StreamReader(await httpClient.GetStreamAsync(uri));
                }
            }

            protected async virtual Task<string> GetStringAsync(Uri uri)
            {
                return await GetStringAsync(uri, GoogleSigned.SigningInstance);
            }

            protected async virtual Task<string> GetStringAsync(Uri uri, GoogleSigned signingInstance)
            {
                if (signingInstance != null)
                {
                    uri = new Uri(signingInstance.GetSignedUri(uri));
                }

                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    return await httpClient.GetStringAsync(uri);
                }
            }

            /// <summary>
            /// Sends a GET request to the <see cref="RequestUri">RequestUri</see> and return the response string in an asynchronous operation.
            /// </summary>
            /// <returns></returns>
            public async virtual Task<string> AsStringAsync()
            {
                return await GetStringAsync(this.RequestUri);
            }

            /// <summary>
            /// Sends a GET request to the RequestUri and return a serialized response object in an asynchronous operation.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public async virtual Task<T> AsAsync<T>() where T : class
            {
                T output = default(T);
                var responseString = await GetStringAsync(this.RequestUri);                
                output = JsonConvert.DeserializeObject<T>(responseString, new JsonEnumTypeConverter(), new JsonLocationConverter());
                
                return output;
            }
        }

        public static HttpGetResponse Get(Uri uri)
        {
            return Factory.CreateResponse(uri);
        }

        /// <summary>
        /// Gets or sets the factory that provides HttpGetResponse instances. Crude depency injection for the time being.
        /// </summary>
        public static HttpGetResponseFactory Factory = new HttpGetResponseFactory();

        /// <summary>
        /// A factory class for building HttpGetResponse instances.
        /// </summary>
        public class HttpGetResponseFactory
        {
            /// <summary>
            /// Builds a standard HttpGetResponse instance.
            /// </summary>
            /// <param name="uri"></param>
            /// <returns></returns>
            public virtual HttpGetResponse CreateResponse(Uri uri)
            {
                return new HttpGetResponse(uri);
            }
        }
    }
}
