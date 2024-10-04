using Cangjie.Core.Extensions;
using System.Collections;

namespace Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;
public class Fields : IEnumerable<Field>
{
    private List<Field> Data { get; set; } = new();

    public Fields Add(params Field[] fields)
    {
        Data.AddRange(fields);
        return this;
    }

    public IEnumerator<Field> GetEnumerator()
    {
        foreach (var i in Data)
        {
            yield return i;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var i in Data)
        {
            yield return i;
        }
    }

    public override string ToString()
    {
        return Data.Join("+");
    }
}
