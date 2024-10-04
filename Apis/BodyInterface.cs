using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Cli.Apis;

public class BodyInterface
{
    public BodyInterface()
    {

    }

    public BodyInterface(Json target)
    {
        Target = target;
    }

    public Json Target { get; set; } = Json.Null;

    
}
