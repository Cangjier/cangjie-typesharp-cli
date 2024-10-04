using Cangjie.Core.Steper;
using Cangjie.Imp.Steper;
using Cangjie.Imp.Steper.StringStep;
using Cangjie.Imp.Steper.ValueSteps;

namespace Cangjie.TypeSharp.Cli.HeaderScript.Steper;
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
