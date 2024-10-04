namespace Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;
public struct Field
{
    public string Value { get; set; }

    public static Fields operator +(Field left, Field right)
    {
        return new Fields().Add(left, right);
    }

    public static Fields operator +(Fields left, Field right)
    {
        return left.Add(right);
    }

    public static Field operator -(Field left, Field right)
    {
        return new() { Value = left.Value + "-" + right.Value };
    }

    public static MimeType operator /(Field left, Field right)
    {
        var result = new MimeType();
        result.Master = left.Value;
        result.Types.Add(right.Value);
        return result;
    }

    public static MimeType operator /(Field left, Fields right)
    {
        var result = new MimeType();
        result.Master = left.Value;
        foreach (var item in right)
        {
            result.Types.Add(item.Value);
        }
        return result;
    }

    public static Key op_Assignment(Field left, Field right)
    {
        return new()
        {
            Name = left.Value,
            Value = right
        };
    }

    public static Key op_Assignment(Field left, double right)
    {
        return new()
        {
            Name = left.Value,
            Value = right
        };
    }

    public static Key op_Assignment(Field left, string right)
    {
        return new()
        {
            Name = left.Value,
            Value = right
        };
    }

    /// <summary>
    /// 封号运算符
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static MimeType op_Semicolon(Field left, Key right)
    {
        var result = new MimeType
        {
            Master = left.Value
        };
        result.Map[right.Name] = right.Value;
        return result;
    }

    public override string ToString()
    {
        return Value;
    }
}
