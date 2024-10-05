namespace  Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;
public struct Key
{
    public string Name { get; set; }

    public object Value { get; set; }

    public override string ToString()
    {
        return $"{Name}={Value}";
    }
}
