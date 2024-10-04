using System.Net.Http.Headers;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Cli.Apis;
public class Client
{
    private static HttpClient HttpClient { get; set; } = new()
    {
        Timeout = TimeSpan.FromDays(1)
    };

    public static async Task<Response> Send(Request request,NetMessageInterface msg)
    {
        HttpRequestMessage requestMessage = new();
        requestMessage.Method = request.Method.ToLower() switch
        {
            "post" => HttpMethod.Post,
            "put" => HttpMethod.Put,
            "get" => HttpMethod.Get,
            "patch" => HttpMethod.Patch,
            "options" => HttpMethod.Options,
            "head" => HttpMethod.Head,
            "trace" => HttpMethod.Trace,
            "delete" => HttpMethod.Delete,
            _ => HttpMethod.Get,
        };
        requestMessage.RequestUri = new Uri(request.UrlWithQueryParameters);
        request.Headers.ForeachObject((key, value) =>
        {
            if (key.ToLower().StartsWith("content-"))
            {
                return;
            }
            requestMessage.Headers.Add(key, value.ToString());
        });
        requestMessage.Content = request.Body.Target.ToHttpContent(msg);
        if (requestMessage.Content != null)
        {
            request.Headers.ForeachObject((key, value) =>
            {
                if (key.ToLower() == "Content-Type".ToLower())
                {
                    requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value.ToString());
                }
                else if (key.ToLower() == "Content-Encoding".ToLower())
                {
                    requestMessage.Content.Headers.ContentEncoding.Add(value.ToString());
                }
                else if (key.ToLower() == "Content-Language".ToLower())
                {
                    requestMessage.Content.Headers.ContentLanguage.Add(value.ToString());
                }
                else if (key.ToLower() == "Content-Location".ToLower())
                {
                    requestMessage.Content.Headers.ContentLocation = new Uri(value.ToString());
                }
                else if (key.ToLower() == "Content-MD5".ToLower())
                {
                    requestMessage.Content.Headers.ContentMD5 = Convert.FromBase64String(value.ToString());
                }
                else if (key.ToLower() == "Content-Range".ToLower())
                {
                    requestMessage.Content.Headers.ContentRange = ContentRangeHeaderValue.Parse(value.ToString());
                }
                else if (key.ToLower() == "Content-Disposition".ToLower())
                {
                    requestMessage.Content.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(value.ToString());
                }
                else if (key.ToLower() == "Content-Length".ToLower())
                {
                    requestMessage.Content.Headers.ContentLength = long.Parse(value.ToString());
                }
                else if (key.ToLower().StartsWith("content-"))
                {
                    requestMessage.Content.Headers.Add(key, value.ToString());
                }
            });
        }
        try
        {
            var response = await HttpClient.SendAsync(requestMessage);
            return await Response.Parse(response);
        }
        catch(Exception e)
        {
            msg.Error("请求异常",e);
            return new Response();
        }
    }
}
