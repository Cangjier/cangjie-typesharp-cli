using System.Net.Http.Headers;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Cli.Apis;
public static class BodyExtensions
{
    public static string[] InnerTypeAlias = ["Type"];

    public static string? GetType(Json self)
    {
        foreach (var item in InnerTypeAlias)
        {
            if (self.ContainsKey(item))
            {
                var result = self.Read(item, string.Empty);
                self.RemoveKey(item);
                return result;
            }
        }
        return null;
    }

    public static HttpContent? ToHttpContent(this Json self,NetMessageInterface msg)
    {
        if (self.IsString)
        {
            return new StringContent(self.AsString);
        }
        else if (self.IsArray)
        {
            return FormDataBodyToHttpContent(self, msg);
        }
        else if (self.IsNull) return null;

        if (!self.IsObject)
        {
            msg.Error("Body must be an object or string");
            return null;
        }

        var type = GetType(self)?.ToLower();
        switch (type)
        {
            case "none":
                return null;
            case "json":
                {
                    var result = new StringContent(self.Get("Data").ToString());
                    result.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return result;
                }
            case "binary":
                return BinaryBodyToHttpContent(self, msg);
            default:
                {
                    var result = new StringContent(self.ToString());
                    result.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return result;
                }
        }
    }

    public static HttpContent FormDataBodyToHttpContent(Json self, NetMessageInterface msg)
    {
        var result = new MultipartFormDataContent();
        self.ForeachArray(item =>
        {
            var itemType = item.Read("Type",string.Empty);
            if (itemType.ToLower() == "file")
            {
                var path = item.Read("Path", string.Empty);
                var name = item.Read("Name", string.Empty);
                var fileName = item.Read("FileName", string.Empty);
                if (File.Exists(path))
                {
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(path));
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    result.Add(fileContent,
                        string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name,
                        string.IsNullOrEmpty(fileName) ? Path.GetFileName(path) : fileName);
                }
                else
                {
                    msg.Error($"File not found, {path}");
                }
            }
            else
            {
                var name = item.Read("Name", string.Empty);
                var value = item.Get("Value").ToString();
                result.Add(new StringContent(value), name);
            }
        });
        return result;
    }

    public static HttpContent? BinaryBodyToHttpContent(Json self,NetMessageInterface msg)
    {
        if (self.ContainsKey("Base64"))
        {
            return new ByteArrayContent(Convert.FromBase64String(self.Read("Base64", string.Empty)));
        }
        else if (self.ContainsKey("Path"))
        {
            var path = self.Read("Path", string.Empty);
            var chunkSizeStr = self.Read("ChunkSize",string.Empty);
            var chunkIndexStr = self.Read("ChunkIndex", string.Empty);
            if (File.Exists(path))
            {
                if(int.TryParse(chunkSizeStr,out int chunkSize)&&
                    int.TryParse(chunkIndexStr,out int chunkIndex))
                {
                    using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var buffer = new byte[chunkSize];
                    stream.Seek(chunkIndex * chunkSize, SeekOrigin.Begin);
                    var readCount = stream.Read(buffer, 0, chunkSize);
                    if (readCount > 0)
                    {
                        return new ByteArrayContent(buffer, 0, readCount);
                    }
                    else
                    {
                        msg.Error(readCount == 0 ? "ReadCount is 0" : "ReadCount is less than 0");
                        return null;
                    }
                }
                else
                {
                    return new ByteArrayContent(File.ReadAllBytes(path));
                }
            }
            else
            {
                msg.Error($"File not found, {path}");
                return null;
            }
        }
        else
        {
            msg.Error($"self is not Base64 or Path, {self}");
            return null;
        }
    }
}
