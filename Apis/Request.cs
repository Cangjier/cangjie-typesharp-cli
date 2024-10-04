using System.Text.Encodings.Web;
using TidyHPC.LiteJson;
using TidyHPC.Extensions;

namespace Cangjie.TypeSharp.Cli.Apis;

public class Request
{
    /// <summary>
    /// Http方法
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Http地址
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 合并Url和查询参数
    /// </summary>
    public string UrlWithQueryParameters
    {
        get
        {
            if (!QueryParameters.IsObject || QueryParameters.Count == 0)
            {
                return Url;
            }

            if (Url.Contains('?'))
            {
                return Url + QueryParameters.GetObjectEnumerable().Join("&", (key, value) => $"{key}={UrlEncoder.Default.Encode(value.ToString())}");
            }
            else
            {
                return Url + "?" + QueryParameters.GetObjectEnumerable().Join("&", (key, value) => $"{key}={UrlEncoder.Default.Encode(value.ToString())}");
            }
        }
    }

    /// <summary>
    /// 查询参数
    /// </summary>
    public Json QueryParameters { get; set; } = Json.Null;

    /// <summary>
    /// 请求头
    /// </summary>
    public Json Headers { get; set; } = Json.NewObject();

    /// <summary>
    /// 请求体
    /// </summary>
    public BodyInterface Body { get; set; } = new BodyInterface();

    public static Request? Parse(Json self,NetMessageInterface msg)
    {
        if (!self.IsObject)
        {
            msg.Error("Request must be an object");
            return null;
        }
        if(self.ContainsKey("Method")&& self.ContainsKey("Url"))
        {
            Request request = new();
            request.Method = self.Read("Method", string.Empty);
            request.Url = self.Read("Url", string.Empty);
            request.QueryParameters = self.GetOrCreateObject("QueryParameters");
            request.Headers = self.GetOrCreateObject("Headers");
            request.Body = new BodyInterface(self.Get("Body",Json.Null));
            return request;
        }
        else
        {
            return null;
        }
    }
}
