using  Cangjie.TypeSharp.Cli.HeaderScript.Steper;
using  Cangjie.Core.Syntax;
using System.Collections;
using  Cangjie.Dawn.Text;
using  Cangjie.Core.Syntax.Templates;
using Cangjie.Owners;
using Cangjie.Dawn.Text.Units;
using Cangjie.Core.Steper;
using Cangjie.Core.Runtime;
using TidyHPC.Extensions;


namespace  Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;
public class MimeTypes:IEnumerable<MimeType>,IReleasable
{
    public static Template<char> MimeTypeTemplate { get; } = new Func<Template<char>>(() =>
    {
        var result = new Template<char>();
        result.BranchTemplate.Ban(LineAnnotation.JumpIn, AreaAnnotation.JumpIn);
        result.SymbolTemplate.Ban('-', '_', '.');
        return result;
    })();

    public static HeaderStepEngine StepEngine { get; } = new HeaderStepEngine();

    public static MimeTypes? Parse(string Content)
    {
        Owner owner = new();
        Document<char> document = new(owner,(Path) => Content[Path], () => Content.Length);
        TextContext textContext = new (owner,MimeTypeTemplate);
        textContext.Process(document);

        StepContext<char> stepContext = new(owner);
        var parseResult = StepEngine.Parse(owner, stepContext, textContext.Root.Data, false);
        var steps = parseResult.Steps;

        HeaderScriptRuntimeContext memoryContext = new();
        steps.Run(memoryContext);

        var lastValue = memoryContext.GetLastObject().Value;
        owner.Release();

        if (lastValue is MimeTypes mimeTypes) return mimeTypes;
        if(lastValue is MimeType mimeType)
        {
            var result = new MimeTypes
            {
                mimeType
            };
            return result;
        }
        else if(lastValue is Field field)
        {
            var result = new MimeTypes
            {
                new MimeType()
                {
                    Master=field.Value
                }
            };
            return result;
        }
        else if(lastValue is null)
        {
            return new();
        }
        else
        {
            return null;
        }
    }

    internal List<MimeType> Data { get; set; } = new();

    public MimeType this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }

    public int Count => Data.Count;

    public IEnumerator<MimeType> GetEnumerator()
    {
        foreach(var i in Data)
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

    public MimeTypes Add(params MimeType[] mimeTypes)
    {
        Data.AddRange(mimeTypes);
        return this;
    }

    public MimeType? Get(string master)
    {
        foreach(var i in Data)
        {
            if(i.Master==master)return i;
        }
        return null;
    }

    public bool Contains(string master)
    {
        foreach (var i in Data)
        {
            if (i.Master == master) return true;
        }
        return false;
    }

    public MimeType GetOrCreate(string master)
    {
        foreach (var i in Data)
        {
            if (i.Master == master) return i;
        }
        var result = new MimeType() 
        {
            Master=master
        };
        Data.Add(result);
        return result;

    }

    public override string ToString()
    {
        return Data.Join(",");
    }

    public void Release()
    {
        foreach(var i in Data)
        {
            i.Release();
        }
        Data.Clear();
        Data = null!;
    }

    public MimeTypes Copy()
    {
        var result = new MimeTypes();
        foreach(var i in Data)
        {
            result.Data.Add(i.Copy());
        }
        return result;
    }
}
