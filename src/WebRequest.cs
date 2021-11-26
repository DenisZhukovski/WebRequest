﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebRequest.Elegant.Core;

namespace WebRequest.Elegant
{
    public sealed class WebRequest : IWebRequest
    {
        private readonly Dictionary<string, string> _queryParams;
        private readonly HttpClient _httpClient;
        private readonly IToken _token;
        private readonly HttpMethod _httpMethod;
        private readonly IBodyContent _body;

        #region Constructors

        public WebRequest(
            string uriString
        ) : this(uriString, new HttpClient())
        {
        }

        public WebRequest(
            string uriString,
            HttpClient httpClient
        ) : this(new UriFromString(uriString), httpClient)
        {
        }

        public WebRequest(
            string uriString,
            HttpMessageHandler messageHandler
        ) : this(new UriFromString(uriString), messageHandler)
        {
        }

        public WebRequest(
            Uri uri,
            HttpMessageHandler messageHandler
        ) : this(new UriFromString(uri), new HttpClient(messageHandler))
        {
        }

        public WebRequest(
            Uri uri,
            HttpClient httpClient
        ) : this(new UriFromString(uri), new HttpAuthenticationHeaderToken(), httpClient)
        {
        }

        public WebRequest(
            Uri uri,
            IToken token,
            HttpClient httpClient
        ) : this(
                new UriFromString(uri),
                token,
                HttpMethod.Get,
                new JsonBodyContent(new EmptyJsonObject()),
                new Dictionary<string, string>(),
                httpClient
            )
        {
        }

        public WebRequest(
            Uri uri,
            IToken token,
            HttpMethod method,
            IBodyContent body
        ) : this(
                new UriFromString(uri),
                token,
                method,
                body,
                new Dictionary<string, string>(),
                new HttpClient()
            )
        {
        }

        public WebRequest(
            Uri uri,
            IToken token,
            HttpMethod method,
            IBodyContent body,
            Dictionary<string, string> queryParams,
            HttpClient httpClient)
            : this(new UriFromString(uri), token, method, body, queryParams, httpClient)
        {
        }

        public WebRequest(
            IUri uri,
            HttpMessageHandler messageHandler
        ) : this(uri, new HttpClient(messageHandler))
        {
        }

        public WebRequest(
            IUri uri,
            HttpClient httpClient
        ) : this(uri, new HttpAuthenticationHeaderToken(), httpClient)
        {
        }

        public WebRequest(
            IUri uri,
            IToken token,
            HttpClient httpClient
        ) : this(
                uri,
                token,
                HttpMethod.Get,
                new JsonBodyContent(new EmptyJsonObject()),
                new Dictionary<string, string>(),
                httpClient
            )
        {
        }

        public WebRequest(
            IUri uri,
            IToken token,
            HttpMethod method,
            IBodyContent body
        ) : this(
                uri,
                token,
                method,
                body,
                new Dictionary<string, string>(),
                new HttpClient()
            )
        {
        }

        public WebRequest(
            IUri uri,
            IToken token,
            HttpMethod method,
            IBodyContent body,
            Dictionary<string, string> queryParams,
            HttpClient httpClient)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _httpMethod = method ?? throw new ArgumentNullException(nameof(method));
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _queryParams = queryParams ?? throw new ArgumentNullException(nameof(queryParams));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        #endregion

        public IUri Uri { get; }

        public async Task<HttpResponseMessage> GetResponseAsync()
        {
            var requestMessage = await RequestMessageAsync(_httpMethod).ConfigureAwait(false);
            try
            {
                return await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            }
            finally
            {
                requestMessage.Dispose();
            }
        }

        public IWebRequest WithPath(IUri uri)
        {
            return new WebRequest(
                uri,
                _token,
                _httpMethod,
                _body,
                _queryParams,
                _httpClient
            );
        }

        public IWebRequest WithMethod(HttpMethod method)
        {
            return new WebRequest(
                Uri,
                _token,
                method,
                _body,
                _queryParams,
                _httpClient
            );
        }

        public IWebRequest WithQueryParams(Dictionary<string, string> parameters)
        {
            return new WebRequest(
                Uri,
                _token,
                _httpMethod,
                _body,
                parameters,
                _httpClient
            );
        }

        public IWebRequest WithBody(IBodyContent body)
        {
            return new WebRequest(
                Uri,
                _token,
                _httpMethod,
                body,
                _queryParams,
                _httpClient
            );
        }

        public override string ToString()
        {
            return $"Uri: {new QueryParamsAsString(_queryParams).With(Uri.Uri())}\n" +
                   $"Token: {_token}\n" +
                   $"Body: {_body}";
        }

        private async Task<HttpRequestMessage> RequestMessageAsync(HttpMethod method)
        {
            var request = new HttpRequestMessage(
                method,
                new QueryParamsAsString(_queryParams).With(Uri.Uri())
            );
            _body.InjectTo(request);
            await _token.InjectToAsync(request).ConfigureAwait(false);
            return request;
        }

        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(this, obj)
                || new TheSameUri(Uri, obj).ToBool()
                || obj is IToken token && _token.Equals(token)
                || obj is HttpMethod method && _httpMethod == method
                || obj is IBodyContent body && _body.Equals(body)
                || TheSameQueryParameters(obj);
        }

        public override int GetHashCode()
        {
#if NETSTANDARD2_1
            return HashCode.Combine(Uri, _token, _httpMethod, _body, _queryParams);
#else
            return new { Uri, _token, _httpMethod, _body, _queryParams }.GetHashCode();
#endif
        }

        private bool TheSameQueryParameters(object obj)
        {
            if (obj is Dictionary<string, string> parameters)
            {
                return new TheSameDictionary<string, string>(
                    _queryParams,
                    parameters
                ).ToBool();
            }

            return false;
        }
    }
}
