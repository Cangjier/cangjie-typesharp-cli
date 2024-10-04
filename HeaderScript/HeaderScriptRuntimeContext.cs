using Cangjie.Core.Runtime;

namespace Cangjie.TypeSharp.Cli.HeaderScript;
internal class HeaderScriptRuntimeContext : RuntimeContext<char>
{
    public HeaderScriptRuntimeContext(IOwner owner) : base(owner)
    {
    }

    public override RuntimeContext<char> Clone()
    {
        return new HeaderScriptRuntimeContext(Owner);
    }
}
