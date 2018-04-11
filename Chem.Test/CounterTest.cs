using System;
using System.Collections.Immutable;
using System.Linq;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Chem.Test
{
  [TestFixture]
  public class CounterTest
  {
    public static (string, (string, int)[], string)[] ApplyTestCases =
    {
      ("", new[] {("", 0)}, ""),
      ("aaaaa", new[] {("a->", 1)}, null),
      ("aaaaa", new[] {("aaa->", 1)}, "aa"),
      ("aaaaa", new[] {("aa->", 1)}, null),
      ("aaaaa", new[] {("aa->", 3)}, null),
      ("aaaaa", new[] {("aa->", 2)}, "a"),
      ("aaaaa", new[] {("aac->", 2)}, null),
      ("aaaaacccc", new[] {("aac->", 2)}, "acc"),
      ("aaaabbbb", new[] {("aabb->c", 2)}, "cc"),
      ("aaaabbbb", new[] {("aabb->c", 3)}, null),
      ("aaaabbbb", new[] {("aabbb->c", 1)}, "aabc"),
      ("aaaabbbb", new[] {("aaabbb->c", 1)}, "abc"),
    };

    [Test]
    public void ApplySingleSet(
      [ValueSource(nameof(ApplyTestCases))] (string counter, (string transform, int count)[] trans, string result) val)
    {
      var str = string.Join(Environment.NewLine, new[] {val.counter}.Concat(val.trans.Select(tra => tra.transform)));
      var (counter, trans) = ChemBuilder.Parse(str);
      var countedTrans = trans.Zip(val.trans.Select(tra => tra.count), (tra, cou) => (Tra: tra, Cou: cou))
        .ToDictionary(p => p.Tra, p => p.Cou);

      var (comparand, _) = val.result != null ? ChemBuilder.Parse(val.result + "\n") : (null, null);

      var result = counter.Apply(countedTrans);

      Console.WriteLine(string.Join(Environment.NewLine,
        new[] {val.counter}.Concat(val.trans.Select(tra => tra.count + ": " + tra.transform))));
      Console.WriteLine(val.result);
      Console.WriteLine(result);

      var logic = new CompareLogic();
      var compareResult = logic.Compare(result, comparand);
      Assert.True(compareResult.AreEqual, compareResult.DifferencesString);
    }

    public static (string, string[], int[][])[] EnumeratedCases =
    {
      ("", new[] {""}, new []{new[]{0}}),
      ("aaaaa", new[] {"a->"}, new []
      {
        new []{0},
        new []{1},
        new []{2},
        new []{3},
        new []{4},
        new []{5},
      }),
      ("aaaaa", new[] {"aa->"}, new []
      {
        new []{0},
        new []{1},
        new []{2},
      }),
      ("aabbb", new[]
      {
        "a->",
        "ab->",
        "b->"
      }, new []
      {
        new []{0,0,0},
        new []{0,0,1},
        new []{0,0,2},
        new []{0,0,3},
        new []{0,1,0},
        new []{0,1,1},
        new []{0,1,2},
        new []{0,1,3},
        new []{0,2,0},
        new []{0,2,1},
        new []{0,2,2},
        new []{0,2,3},
        new []{1,0,0},
        new []{1,0,1},
        new []{1,0,2},
        new []{1,0,3},
        new []{1,1,0},
        new []{1,1,1},
        new []{1,1,2},
        new []{1,1,3},
        new []{1,2,0},
        new []{1,2,1},
        new []{1,2,2},
        new []{1,2,3},
        new []{2,0,0},
        new []{2,0,1},
        new []{2,0,2},
        new []{2,0,3},
        new []{2,1,0},
        new []{2,1,1},
        new []{2,1,2},
        new []{2,1,3},
        new []{2,2,0},
        new []{2,2,1},
        new []{2,2,2},
        new []{2,2,3},
      }),
    };

    [Test]
    public void GetEnumeratedSets(
      [ValueSource(nameof(EnumeratedCases))] (string counter, string[] transforms, int[][] counts) val)
    {
      var str = string.Join(Environment.NewLine, new[] { val.counter }.Concat(val.transforms));
      var (counter, trans) = ChemBuilder.Parse(str);

      var expected = val.counts.Select(arr =>
        arr.Zip(trans, (i, transform) => (I: i, Trans: transform)).ToDictionary(v => v.Trans, v => v.I));

      var result = counter.PossibleInputs(trans);

      var logic = new CompareLogic
      {
        Config =
        {
          IgnoreObjectTypes = true,
          IgnoreCollectionOrder = true
        }
      };
      var compareResult = logic.Compare(expected, result);
      Assert.True(compareResult.AreEqual, compareResult.DifferencesString);
    }
  }
}
