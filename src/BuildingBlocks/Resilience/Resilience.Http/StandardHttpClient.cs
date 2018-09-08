using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;

namespace Resilience.Http
{
    public class StandardHttpClient : IHttpClient
    {
        //the reason for having httpclient as static is 
        //As a first issue, while this class is disposable, using it with the using statement is not the best choice because 
        //even when you dispose HttpClient object, the underlying socket is not immediately released and can cause a serious issue named ‘sockets exhaustion’. 
        //https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        private static HttpClient _client = new HttpClient();

        private readonly ILogger<StandardHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StandardHttpClient(ILogger<StandardHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            //_client = new HttpClient();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }
        }
        public async Task<string> GetStringAsync(string uri, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            SetAuthorizationHeader(requestMessage);
            if(authorizationToken != null)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
            }

            var response = await _client.SendAsync(requestMessage);

            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException();
            }
            
            return await response.Content.ReadAsStringAsync();
        }
        
        public async Task<HttpResponseMessage> PutOrPostAsync<T>(HttpMethod httpMethod, string uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            if(httpMethod != HttpMethod.Put && httpMethod != HttpMethod.Post)
            {
                throw new ArgumentException($"passed method must be either put or post and not {nameof(httpMethod)}");
            }

            var requestMessage = new HttpRequestMessage(httpMethod, uri);
            SetAuthorizationHeader(requestMessage);

            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");

            if(authorizationToken != null)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
            }

            if(requestId != null)
            {
                requestMessage.Headers.Add("x-requestid", requestId);
            }

            var response = await _client.SendAsync(requestMessage);

            if(response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException();
            }
            return response;
        }
        

        public async Task<HttpResponseMessage> PostAsync<T>(string uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return await PutOrPostAsync(HttpMethod.Post, uri, item, authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return await PutOrPostAsync(HttpMethod.Put, uri, item, authorizationToken, requestId, authorizationMethod);
        }
        public async Task<HttpResponseMessage> DeleteAsync(string uri, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);

            SetAuthorizationHeader(requestMessage);

            if(authorizationToken != null)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
            }

            if(requestId != null)
            {
                requestMessage.Headers.Add("x-requestid", requestId);
            }

            return await _client.SendAsync(requestMessage);
        }
    }
}
