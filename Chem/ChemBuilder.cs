using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Chem
{
  public static class ChemBuilder
  {
    public static (Counter Initial, List<Transform> Transforms) Parse(string expression)
    {
      var lexer = new ChemsLexer(new AntlrInputStream(expression));
      var parser = new ChemsParser(new CommonTokenStream(lexer));
      var prog = parser.program();
      var visits = new ChemVisitor();
      return visits.Visit(prog);
    }

    private class ChemVisitor : ChemsBaseVisitor<(Counter Initial, List<Transform> Transforms)>
    {
      public override (Counter Initial, List<Transform> Transforms) VisitProgram(ChemsParser.ProgramContext context)
      {
        var initials = context.initial().token().GroupBy(tok => tok.GetText()).ToDictionary(ite => ite.Key, ite => ite.Count());
        var counter = new Counter(initials);
        var rv = new RuleVisitor();
        var transforms = context.rule().Select(rv.VisitRule).ToList();
        return (counter, transforms);
      }
    }

    private class RuleVisitor : ChemsBaseVisitor<Transform>
    {
      public override Transform VisitRule(ChemsParser.RuleContext context)
      {
        var required = context.required().token().GroupBy(tok => tok.GetText()).ToDictionary(ite => ite.Key, ite => ite.Count());
        var provided = context.provided().token().GroupBy(tok => tok.GetText()).ToDictionary(ite => ite.Key, ite => ite.Count());
        return new Transform(required, provided);
      }
    }
  }
}
