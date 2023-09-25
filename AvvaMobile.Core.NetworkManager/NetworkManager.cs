using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AvvaMobile.Core
{
    public class HttpResponse
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Exception Exception { get; set; }

        public JsonSerializerOptions SerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    }

    public class HttpResponse<T> : HttpResponse
    {
        public T Data { get; set; }
    }

    public class NetworkManager
    {
        private readonly HttpClient client = new HttpClient();

        public NetworkManager()
        {
        }

        public NetworkManager(string baseAddress)
        {
            SetBaseAddress(baseAddress);
        }

        /// <summary>
        /// Updates the base address of http client.
        /// </summary>
        /// <param name="baseAddress"></param>
        public void SetBaseAddress(string baseAddress)
        {
            client.BaseAddress = new Uri(baseAddress);
        }

        /// <summary>
        /// Clears all existing header from the client.
        /// </summary>
        public void ClearHeaders()
        {
            client.DefaultRequestHeaders.Clear();
        }

        /// <summary>
        /// Adds new header value to the client request.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value)
        {
            client.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Adds Bearer token to header.
        /// </summary>
        /// <param name="token"></param>
        public void AddBearerToken(string token)
        {
            AddHeader("Authorization", $"Bearer {token}");
        }

        /// <summary>
        /// Adds "ContentType:application/json" header to current request.
        /// </summary>
        public void AddContentTypeJSONHeader()
        {
            AddHeader("ContentType", "application/json");
        }

        /// <summary>
        /// Sends a GET request and returns data as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> GetAsync<T>(string uri)
        {
            return await GetAsync<T>(uri, null);
        }

        /// <summary>
        /// Sends a GET request with url parameters and returns data as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> GetAsync<T>(string uri, Dictionary<string, string> parameters)
        {
            var response = new HttpResponse<T>();

            try
            {
                var sb = new StringBuilder();
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var param in parameters)
                    {
                        sb.Append(param.Key);
                        sb.Append("=");
                        sb.Append(param.Value);
                        sb.Append("&");
                    }
                }

                uri = uri.IndexOf("?") > -1 ? $"{client.BaseAddress}{uri}&{sb}" : $"{client.BaseAddress}{uri}?{sb}";

                var resp = await client.GetAsync(uri);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = await resp.Content.ReadFromJsonAsync<T>(response.SerializerOptions);
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.GetAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Downloads the file to given path.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="outputPath"></param>
        /// <returns></returns>
        public async Task<HttpResponse> DownloadFile(string url, string outputPath)
        {
            var response = new HttpResponse();

            try
            {
                var fileBytes = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(outputPath, fileBytes);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.DownloadFile Error: " + ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Exception = ex;
            }

            return response;
        }

        public async Task<HttpResponse<string>> GetXMLStringAsync(string uri)
        {
            return await GetXMLStringAsync(uri, null);
        }

        public async Task<HttpResponse<string>> GetXMLStringAsync(string uri, Dictionary<string, string> parameters)
        {
            var response = new HttpResponse<string>();

            try
            {
                var sb = new StringBuilder();
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var param in parameters)
                    {
                        sb.Append(param.Key);
                        sb.Append("=");
                        sb.Append(param.Value);
                        sb.Append("&");
                    }
                }

                if (uri.IndexOf("?") > -1)
                {
                    uri = $"{client.BaseAddress}{uri}&{sb}";
                }
                else
                {
                    uri = $"{client.BaseAddress}{uri}?{sb}";
                }

                var resp = await client.GetAsync(uri);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = responseString;
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.GetAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Sends a POST request with data object. Also returns http response as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> PostAsync<T>(string uri, dynamic data)
        {
            var response = new HttpResponse<T>();

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync($"{client.BaseAddress}{uri}", content);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = await resp.Content.ReadFromJsonAsync<T>(response.SerializerOptions);
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.PostAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }
        
        /// <summary>
        /// Sends a POST request with data object. Also returns http response as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponse> PostAsync(string uri, dynamic data)
        {
            var response = new HttpResponse();

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync($"{client.BaseAddress}{uri}", content);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                
                if (response.IsSuccess)
                {
                    
                }
                else
                {
                    var responseString = await resp.Content.ReadAsStringAsync();
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.PostAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Sends a POST request with encoded data object (x-www-form-urlencoded). Also returns http response as T type value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> PostEncodedAsync<T>(string uri, Dictionary<string, string> data)
        {
            var response = new HttpResponse<T>();

            try
            {
                var resp = await client.PostAsync($"{client.BaseAddress}{uri}", new FormUrlEncodedContent(data));
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = await resp.Content.ReadFromJsonAsync<T>(response.SerializerOptions);
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.PostAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Sends a PUT request with data object. Also returns http response as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> PutAsync<T>(string uri, dynamic data)
        {
            var response = new HttpResponse<T>();

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PutAsync($"{client.BaseAddress}{uri}", content);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = await resp.Content.ReadFromJsonAsync<T>(response.SerializerOptions);
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.PutAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Sends a POST request with form data. Also returns http response as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> PostAsFormDataAsync<T>(string uri, MultipartFormDataContent content)
        {
            var response = new HttpResponse<T>();
            try
            {
                var resp = await client.PostAsync($"{client.BaseAddress}{uri}", content);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = await resp.Content.ReadFromJsonAsync<T>(response.SerializerOptions);
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.PostAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Sends a DELETE request with data object. Also returns http response as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> DeleteAsync<T>(string uri)
        {
            var response = new HttpResponse<T>();

            try
            {
                var resp = await client.DeleteAsync(uri);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = JsonConvert.DeserializeObject<T>(responseString);
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.DeleteAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Sends a PATCH request with data object. Also returns http response as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponse<T>> PatchAsync<T>(string uri, dynamic data)
        {
            var response = new HttpResponse<T>();

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PatchAsync($"{client.BaseAddress}{uri}", content);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;
                var responseString = await resp.Content.ReadAsStringAsync();
                if (response.IsSuccess)
                {
                    response.Data = await resp.Content.ReadFromJsonAsync<T>(response.SerializerOptions);
                }
                else
                {
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.PatchAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Sends a PATCH request with data object. Also returns http response as String value.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponse> PatchAsync(string uri, dynamic data)
        {
            var response = new HttpResponse();

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PatchAsync($"{client.BaseAddress}{uri}", content);
                response.IsSuccess = resp.IsSuccessStatusCode;
                response.StatusCode = resp.StatusCode;

                if (response.IsSuccess)
                {

                }
                else
                {
                    var responseString = await resp.Content.ReadAsStringAsync();
                    response.Message = responseString;
                }
            }
            catch (HttpRequestException ex)
            {
                response.IsSuccess = false;
                response.Message = "HttpClient.PostAsync Error: " + ex.Message;
                response.Exception = ex;
            }

            return response;
        }
    }
}