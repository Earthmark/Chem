using System.Collections.Generic;
using System.Linq;

namespace Chem
{
  public class Transform
  {
    public Dictionary<string, int> Inputs { get; }
    public Dictionary<string, int> Outputs { get; }

    public Transform(Dictionary<string, int> inputs, Dictionary<string, int> outputs)
    {
      Inputs = inputs;
      Outputs = outputs;
    }

    public override string ToString() =>
      $"{string.Join("", Inputs.SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)))}->{string.Join("", Outputs.SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)))}";
  }
}
