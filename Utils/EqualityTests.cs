using CarolCustomizer.Models.Outfits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarolCustomizer.Utils;
internal class EqualityTests
{
    //test reflexive property
    //x.Equals(x) == true

    //test symmetric property
    //y.equals(y) == true

    //test transitive property
    //x.equals(y) && y.equals(x) => x.equals(z)

    //equality holds for every execution as long as values don't change

    //non-null values do not equal null, and x.equals(y) throws when x is null

    internal static void SourceDescriptorTest()
    {
        var name1 = "source one";
        var name2 = "source two";
        var x = new SourceDescriptor(name1, Models.SourceType.Outfit);
        var y = new SourceDescriptor(name1, Models.SourceType.Outfit);
        var z = new SourceDescriptor(name1, Models.SourceType.Outfit);
        var a = new SourceDescriptor(name2, Models.SourceType.Outfit);
        var b = new SourceDescriptor(name1, Models.SourceType.World);
        var c = new SourceDescriptor(name2, Models.SourceType.World);
        var n = (SourceDescriptor)null;

        List<SourceDescriptor> list = [x, y, z, a, b, c];
        list
            .Select(i => i.GetHashCode())
            .ForEach(hash => Log.Debug(hash.ToString()));


        AssertTrue(x.Equals(x), "reflexive");
        AssertTrue(!x.Equals(a), "inequality");
        AssertTrue(x.Equals(y) && y.Equals(x), "symmetric");
        AssertTrue(x.Equals(y) && y.Equals(z) && x.Equals(z), "transitive");
        list.ForEach(_ => AssertTrue(x.Equals(y), "execution"));
        AssertTrue(!x.Equals(null), "null inequality");
        try { n.Equals(y); }
        catch { Log.Info("calling equals on null correctly caused an exception"); }

        Log.Info("Testing operator overloads");

        #pragma warning disable CS1718 // Comparison made to same variable
        AssertTrue(x == x, "reflexive");
        #pragma warning restore CS1718 // Comparison made to same variable
        AssertTrue(x != a, "inequality");
        AssertTrue(x == y && y == x, "symmetric");
        AssertTrue(x == y && y == z && x == z, "transitive");
        list.ForEach(_ => AssertTrue(x == y, "execution"));
        AssertTrue(x != null, "null inequality");
        try { var r = n == y; }
        catch { Log.Info("calling equals on null correctly caused an exception"); }

        Log.Info("Test complete!");
    }

    static void AssertTrue(bool testResult, string testName)
    {
        if (testResult) { Log.Info(testName + " passed."); }
        else Log.Error(testName + " failed");
    }
}
