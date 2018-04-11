using System;
using System.IO;
using System.Linq;

namespace Chem
{
  class Program
  {
    static void Main(string[] args)
    {
      var text = File.ReadAllText(args[0]);
      var (counter, transforms) = ChemBuilder.Parse(text);

      var areEqual =
        from s1 in counter.Apply(transforms)
        from s2 in s1.Apply(transforms)
        from s3 in s2.Apply(transforms)
        where s3.Values.Select(kvp => kvp.Value as int?).Aggregate((v1, v2) => v1 == v2 ? v1 : null) != null
        select (s1, s2, s3);

      foreach (var result in areEqual.Select(ite => ite.ToString()))
      {
        Console.WriteLine(result);
      }
    }
  }
}
