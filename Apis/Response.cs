using  Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;
using System.IO.Compression;
using System.Text;
using System.Web;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Cli.Apis;
public class Response
{
    /// <summary>
    /// 状态代码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 响应头
    /// </summary>
    public Json Headers { get; set; } = Json.NewObject();

    public MimeTypes? ContentType
    {
        get
        {
            if (!Headers.ContainsKey("Content-Type"))
            {
                return null;
            }
            return MimeTypes.Parse(Headers.Read("Content-Type", string.Empty));
        }
    }

    public MimeTypes? ContentDisposition
    {
        get
        {
            if (!Headers.ContainsKey("Content-Disposition"))
            {
                return null;
            }
            return MimeTypes.Parse(Headers.Read("Content-Disposition", string.Empty));
        }
    }

    public string ContentEncoding
    {
        get => Headers.Read("Content-Encoding", string.Empty);
    }

    public bool ContentIsJson
    {
        get
        {
            var contentType = ContentType;
            if (contentType == null) return false;
            MimeType? multipartMimeType = null;
            foreach (var i in contentType)
            {
                if (i.Master == "application")
                {
                    multipartMimeType = i;
                    break;
                }
            }
            if (multipartMimeType == null) return false;
            if (!multipartMimeType.Types.Contains("json")) return false;
            return true;
        }
    }

    public bool ContentIsForm
    {
        get
        {
            var contentType = ContentType;
            if (contentType == null) return false;
            MimeType? multipartMimeType = null;
            foreach (var i in contentType)
            {
                if (i.Master == "multipart")
                {
                    multipartMimeType = i;
                    break;
                }
            }
            if (multipartMimeType == null) return false;
            if (!multipartMimeType.Types.Contains("form-data") || !multipartMimeType.Map.ContainsKey("boundary")) return false;
            var boundaryValue = multipartMimeType.Map["boundary"];
            string? boundary = null;
            if (boundaryValue is string) boundary = boundaryValue as string;
            if (boundaryValue is Field field) boundary = field.Value;
            if (boundary == null) return false;
            return true;
        }
    }

    /// <summary>
    /// 响应体
    /// </summary>
    public object? Body { get; set; } = null;

    public static async Task<Response> Parse(HttpResponseMessage httpResponseMessage)
    {
        Response result = new();
        result.StatusCode = (int)httpResponseMessage.StatusCode;
        foreach (var item in httpResponseMessage.Headers)
        {
            result.Headers.Set(item.Key, string.Join(",", item.Value));
        }
        foreach(var item in httpResponseMessage.Content.Headers)
        {
            result.Headers.Set(item.Key, string.Join(",", item.Value));
        }
        if (result.ContentEncoding == string.Empty)
        {
            result.Body = await httpResponseMessage.Content.ReadAsByteArrayAsync();
        }
        else if (result.ContentEncoding == "gzip")
        {
            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            await gzipStream.CopyToAsync(memoryStream);
            result.Body = memoryStream.ToArray();
        }
        else if (result.ContentEncoding == "deflate")
        {
            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            await deflateStream.CopyToAsync(memoryStream);
            result.Body = memoryStream.ToArray();
        }
        else if (result.ContentEncoding == "br")
        {
            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var brStream = new BrotliStream(stream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            await brStream.CopyToAsync(memoryStream);
            result.Body = memoryStream.ToArray();
        }
        else
        {
            throw new Exception("unkown content encoding");
        }

        return result;
    }

    private static UTF8Encoding UTF8 { get; } = new(false);

    public Json ToJson(Treatment treatment)
    {
        Json result = Json.NewObject();
        result.Set("StatusCode", StatusCode);
        result.Set("Headers", Headers);
        if(Body is byte[] bodyBytes)
        {
            if (
                ContentDisposition is MimeTypes contentDisposition &&
                contentDisposition.Get("attachment") is MimeType attachment &&
                attachment.TryGetString("filename",out var filename))
            {
                string path;
                filename = HttpUtility.UrlDecode(filename);
                if (treatment.Parameters.ContainsKey("DefaultDownloadPath"))
                {
                    path = treatment.Parameters.Read("DefaultDownloadPath",string.Empty);
                }
                else if(treatment.Parameters.ContainsKey("DefaultDownloadDirectory"))
                {
                    path = Path.Combine(treatment.Parameters.Read("DefaultDownloadDirectory", string.Empty),filename);
                }
                else
                {
                    var tempDirectory = Path.GetTempPath() + Guid.NewGuid().ToString("N").ToUpperInvariant();
                    Directory.CreateDirectory(tempDirectory);
                    path = Path.Combine(tempDirectory, filename);
                }
                File.WriteAllBytes(path, bodyBytes);
                result.Set("Body", path);
            }
            else if (ContentIsJson)
            {
                result.Set("Body", Json.Parse(UTF8.GetString(bodyBytes)));
            }
            else if (ContentIsForm)
            {
                var path = Path.GetTempFileName();
                File.WriteAllBytes(Path.GetTempFileName(), bodyBytes);
                result.Set("Body", path);
            }
            else
            {
                var str = UTF8.GetString(bodyBytes);
                if (Json.Validate(str))
                {
                    result.Set("Body", Json.Parse(UTF8.GetString(bodyBytes)));
                }
                else
                {
                    result.Set("Body", UTF8.GetString(bodyBytes));
                }
            }
        }
        else if(Body is Json bodyTson)
        {
            result.Set("Body", bodyTson);
        }
        else
        {
            throw new Exception("unkown body");
        }
        return result;
    }

    public Json ToNativeTson()
    {
        Json result = Json.NewObject();
        result.Set("StatusCode", StatusCode);
        result.Set("Headers", Headers);
        if(Body is null)
        {
            result.Set("Body", Json.Null);
        }
        else
        {
            result.Set("Body", Body);
        }
        return result;
    }

    public void FromNativeTson(Json value)
    {
        var body = value.Get("Body");
        Body = value.Node;
    }
}
