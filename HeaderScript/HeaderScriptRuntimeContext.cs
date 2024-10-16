
using Cangjie.Core.Runtime;
using Cangjie.Owners;

namespace Cangjie.TypeSharp.Cli.HeaderScript;
internal class HeaderScriptRuntimeContext : RuntimeContext<char>
{
    public HeaderScriptRuntimeContext() : base()
    {
    }

    public override RuntimeContext<char> CatchClone()
    {
        return new HeaderScriptRuntimeContext();
    }

    public override RuntimeContext<char> CatchClone(List<string> catchFields)
    {
        return new HeaderScriptRuntimeContext();
    }
}
