using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sunstealer.FunctionApp1;

public static class HttpUtils
{
    public static Task<JObject> Delete(string url, Dictionary<string, string>? headers = null) =>
        ExecuteAsync<JObject>(url, null, HttpMethod.Put, headers);
    
    public static async Task<TOut> Get<TOut>(string url, Dictionary<string, string>? headers = null) =>
        await ExecuteAsync<TOut>(url, null, HttpMethod.Get, headers);

    public static Task<TOut> Post<TIn, TOut>(string url, TIn obj, Dictionary<string, string>? headers = null) =>
        ExecuteAsync<TOut>(url, JsonConvert.SerializeObject(obj), HttpMethod.Post, headers);

    public static Task<TOut> Put<TIn, TOut>(string url, TIn obj, Dictionary<string, string>? headers = null) =>
        ExecuteAsync<TOut>(url, JsonConvert.SerializeObject(obj), HttpMethod.Put, headers);

    public static async Task<TOut> ExecuteAsync<TOut>(string url, string? json,
        HttpMethod method, Dictionary<string, string>? headers = null, string contentType = "application/json")
    {
        headers ??= new Dictionary<string, string>();
        foreach (var item in Flow.GetContextAsDictionary())
        {
            headers.Add(item.Key, item.Value);
        }

        var request = CreateRequestToBeSent(url, json, method, headers, contentType);

        var response = await HttpClientFactory.Create().SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsAsync<TOut>();
        }

        throw new Exception($"Status code: {response.StatusCode}, reason: {response.ReasonPhrase}");
    }

    private static HttpRequestMessage CreateRequestToBeSent(string endpoint, string? json,
        HttpMethod method, Dictionary<string, string> headers, string contentType = "application/json")
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(endpoint),
            Method = method,
            Content = json != null ? new StringContent(json, Encoding.UTF8, contentType) : null
        };

        foreach (var item in headers)
        {
            request.Headers.Add(item.Key, item.Value);
        }

        return request;
    }

}

