using TidyHPC.Extensions;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.Cli.Apis;
public class Treatment
{
    public Treatment(Json parameters)
    {
        Parameters = parameters;
    }

    public Json Parameters { get; set; } = Json.Null;

    public void RunCommands(Json commands)
    {
        commands.EachAll((value) =>
        {
            if (value.IsString)
            {
                var result = EvalString(value.AsString);
                Parameters.Set("ans", result);
                return true;
            }
            else
            {
                return value;
            }
        });
    }

    public Json EvalString(string script)
    {
        return TSScriptEngine.Run(script, stepContext =>
        {
            stepContext.IsSupportDefaultField = true;
        }, runtimeContext =>
        {
            runtimeContext.MountVariableSpace(Parameters);
        });
    }

    public void Process(Json self, string[] skipKeys)
    {
        self.EachAll((path,subValue) =>
        {
            if (skipKeys.Contains(path.First.ToString())) return subValue;
            if (subValue.IsString)
            {
                if (subValue.AsString.StartsWith('$'))
                {
                    var result = EvalString(subValue.AsString[1..]);
                    return result;
                }
            }
            return subValue;
        });
    }

    public void InitialParameters(string? startupPath)
    {
        Parameters.Set("StartupPath", startupPath ?? "");
        Parameters.EachAll((subValue) =>
        {
            if (subValue.IsString)
            {
                if (subValue.AsString.StartsWith('$'))
                {
                    var result = EvalString(subValue.AsString[1..]);
                    return result;
                }
            }
            return subValue;
        });
    }

    public void CoverParametersBy(Json parameters)
    {
        if (parameters.IsObject)
        {
            parameters.GetObjectEnumerable().Foreach(pair =>
            {
                Parameters.Set(pair.Key, pair.Value.Clone());
            });
        }
    }
}
