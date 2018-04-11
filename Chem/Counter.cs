using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Chem
{
  public class Counter
  {
    public Dictionary<string, int> Values { get; }

    public Counter(Dictionary<string, int> values)
    {
      Values = values;
    }

    public IEnumerable<Counter> Apply(List<Transform> transforms)
    {
      return from inputSet in PossibleInputs(transforms) let applied = Apply(inputSet) where applied != null select applied;
    }

    private static TValue GetOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key) =>
      dict.TryGetValue(key, out var taken) ? taken : default(TValue);

    public IEnumerable<IDictionary<Transform, int>> PossibleInputs(IEnumerable<Transform> transforms)
    {
      // fold over int instead of transform, need to map out to span instead of transforms.
      var outer = from possible in transforms
        let viable =
          possible.Inputs.Min(pair => pair.Value != 0 ? (GetOrDefault(Values, pair.Key) / pair.Value) + 1 : 0)
        select from count in Enumerable.Range(0, viable) select (T: possible, C: count);

      IEnumerable<ImmutableDictionary<Transform, int>> baseCase = new[] {ImmutableDictionary<Transform, int>.Empty};

      var expanded = outer.Aggregate(baseCase, (accum, t) =>
        from prev in accum
        from p in t
        select prev.Add(p.T, p.C));
      return expanded;
    }

    public Counter Apply(IDictionary<Transform, int> toApply)
    {

      var toTake = new Dictionary<string, int>(Values);
      foreach (var (name, value) in from kvp in toApply
        from target in kvp.Key.Inputs
        select (target.Key, kvp.Value * target.Value))
      {
        toTake[name] = GetOrDefault(toTake, name) - value;
      }

      var isOk = toTake.Values.All(val => val >= 0) &&
                 !toApply.Keys.Any(tra => tra.Inputs.All(pair => pair.Value != 0 && pair.Value <= toTake[pair.Key]));

      if (!isOk)
      {
        return null;
      }

      var toGive = toTake;
      foreach (var (name, value) in from kvp in toApply
        from target in kvp.Key.Outputs
        select (target.Key, kvp.Value * target.Value))
      {
        toTake[name] = GetOrDefault(toTake, name) + value;
      }

      return new Counter(toGive.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    public override string ToString() =>
      string.Join("", Values.SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)));
  }
}
