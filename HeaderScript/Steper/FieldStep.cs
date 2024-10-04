using Cangjie.Core.Extensions;
using Cangjie.Core.Runtime;
using Cangjie.Core.Steper;
using Cangjie.Core.Syntax;
using Cangjie.Imp.Text.Units;
using Cangjie.TypeSharp.Cli.HeaderScript.Bussiness;

namespace Cangjie.TypeSharp.Cli.HeaderScript.Steper;
public class FieldStep : Step<char>
{
    public class Parser : StepParser<char>
    {
        public override bool Previous(IOwner owner, StepContext<char> context, List<StepParserUnit<char>> units, StepParserUnitIndex index)
        {
            if (units[index].IsSyntaxBase && units[index].SyntaxBase is Common common && !common.IsNumber())
            {
                return true;
            }
            else if (units[index].IsSyntaxBase && units[index].SyntaxBase is Symbol symbol && symbol.TempToString() == "*")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Process(IOwner owner, StepParserEngine<char> engine, StepContext<char> context, Steps<char> steps, List<StepParserUnit<char>> units, ref StepParserUnitIndex index)
        {
            var item = units[index].BaseAs<Block<char>>();
            var result = steps.Create<FieldStep>((step) =>
            {
                step.Set(item.TempToString());
                step.Sign(item);
            });
            units.ReplaceAt(index, new(result));
        }
    }

    public string Raw { get; set; } = string.Empty;

    public Field Value { get; set; }

    public void Set(string Raw)
    {
        this.Raw = Raw;
        Value = new() { Value = Raw };
    }

    public FieldStep(IOwner owner, Steps<char> Parent) : base(owner, Parent)
    {
        MetaData = typeof(Field);
    }

    public override StepResult<char> Run(RuntimeContext<char> Context)
    {
        return new()
        {
            Type = StepResultTypes.Next,
            CacheValue = new()
            {
                Type = MetaData.TryGetType(),
                Value = Value
            }
        };
    }
}
