using Cangjie.Core.Steper;
using  Cangjie.Dawn.Steper;
using  Cangjie.Dawn.Steper.StringSteps;
using  Cangjie.Dawn.Steper.ValueSteps;

namespace  Cangjie.TypeSharp.Cli.HeaderScript.Steper;
public class HeaderStepEngine : StepParserEngine<char>
{
    public HeaderStepEngine()
    {

    }

    public override void Initial()
    {
        AddSingleStepParser(new NumberStep.Parser(), new BooleanStep.Parser(), new FieldStep.Parser()).Add(StringStep.Parsers);

        CreateRank().Add(new BinaryOperator.Parser("+"));
        CreateRank().Add(new BinaryOperator.Parser("/"));
        CreateRank().Add(new BinaryOperator.Parser("="));
        CreateRank().Add(new BinaryOperator.Parser(new Dictionary<string, string?>() { { ";", "op_Semicolon" } }));
        CreateRank().Add(new BinaryOperator.Parser(new Dictionary<string, string?>() { { ",", "op_Comma" } }));
    }
}
