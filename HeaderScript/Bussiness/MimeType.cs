using System.Diagnostics.CodeAnalysis;
using System.Text;
using TidyHPC.Extensions;

namespace Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;
public class MimeType : IReleasable
{
    public string Master { get; set; } = string.Empty;

    public List<string> Types { get; private set; } = new();

    public Dictionary<string, object> Map { get; private set; } = new();

    public bool ContainsType(string type) => Types.Contains(type);

    public bool ContainsKey(string keyName) => Map.ContainsKey(keyName);

    public MimeType Add(params string[] types)
    {
        Types.AddRange(types);
        return this;
    }

    public MimeType Set(string keyName, object value)
    {
        Map[keyName] = value;
        return this;
    }

    public object Get(string keyName) => Map[keyName];

    public bool TryGetString(string keyName, [MaybeNullWhen(false)] out string result)
    {
        if (!Map.ContainsKey(keyName))
        {
            result = null;
            return false;
        }
        else
        {
            var value = Map[keyName];
            if (value is string str)
            {
                result = str;
                return true;
            }
            else if (value is Field field)
            {
                result = field.Value;
                return true;
            }
            else
            {
                result = null;
                return false;
            }

        }
    }

    public object GetOrCreate(string keyName, object defaultValue)
    {
        if (!Map.ContainsKey(keyName))
        {
            Map[keyName] = defaultValue;
            return defaultValue;
        }
        else
        {
            return Map[keyName];
        }
    }

    /// <summary>
    /// 封号运算符
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static MimeType op_Semicolon(MimeType left, Key right)
    {
        left.Map[right.Name] = right.Value;
        return left;
    }

    /// <summary>
    /// 逗号运算符
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static MimeTypes op_Comma(MimeType left, MimeType right)
    {
        var result = new MimeTypes
        {
            left,
            right
        };
        return result;
    }

    /// <summary>
    /// 逗号运算符
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static MimeTypes op_Comma(MimeTypes left, MimeType right)
    {
        left.Add(right);
        return left;
    }

    public override string ToString()
    {
        StringBuilder result = new();
        if (Types.Count == 0)
        {
            result.Append(Master);
        }
        else
        {
            result.Append($"{Master}/{Types.Join("+")}");
        }
        foreach (var i in Map)
        {
            if (i.Value is string)
            {
                result.Append($"; {i.Key}=\"{i.Value}\"");
            }
            else
            {
                result.Append($"; {i.Key}={i.Value}");
            }
        }
        return result.ToString();
    }

    public void Release()
    {
        Master = null!;

        Types.Clear();
        Types = null!;

        Map.Clear();
        Map = null!;
    }

    public MimeType Copy()
    {
        var result = new MimeType()
        {
            Master = Master
        };
        result.Types.AddRange(Types);
        foreach (var i in Map)
        {
            result.Map.Add(i.Key, i.Value);
        }
        return result;
    }
}
