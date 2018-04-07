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

    public IEnumerable<Counter> Apply(List<Transform> transforms, int span)
    {
      // fold over int instead of transform, need to map out to span instead of transforms.
      var outer = from possible in transforms select 
        from count in Enumerable.Range(0, span) select (T: possible, C: count);

      IEnumerable<ImmutableDictionary<Transform, int >> baseCase = new[] {ImmutableDictionary<Transform, int>.Empty};

      var expanded = outer.Aggregate(baseCase, (accum, t) =>
        from prev in accum
        from p in t
        select prev.Add(p.T, p.C));

      return from inputSet in expanded let applied = Apply(inputSet) where applied != null select applied;
    }

    public Counter Apply(IDictionary<Transform, int> toApply)
    {
      TValue GetOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key) =>
        dict.TryGetValue(key, out var taken) ? taken : default(TValue);

      var toConsume =
        (from token in Values.Keys
         from kvp in toApply
         let toTake = GetOrDefault(kvp.Key.Inputs, token) * kvp.Value
         let toGive = GetOrDefault(kvp.Key.Outputs, token) * kvp.Value
         group (Take: toTake, Give: toGive) by token into byTrans
         select (Token: byTrans.Key, Changes: (Take: byTrans.Sum(ite => ite.Take), Give: byTrans.Sum(ite => ite.Give))))
        .ToDictionary(ite => ite.Token, ite => ite.Changes);

      if (toConsume.All(kvp => Values.TryGetValue(kvp.Key, out var has) && has >= kvp.Value.Take))
      {
        var next = Values.ToDictionary(kvp => kvp.Key, kvp =>
        {
          toConsume.TryGetValue(kvp.Key, out var pair);
          var (take, give) = pair;
          return kvp.Value - take + give;
        });

        return new Counter(next);
      }

      return null;
    }

    public override string ToString() =>
      string.Join("", Values.SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)));
  }
}
