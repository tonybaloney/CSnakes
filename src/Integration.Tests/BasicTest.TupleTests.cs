using CSnakes.Runtime;

namespace Integration.Tests;

public partial class BasicTest
{
    [Fact]
    public void RunTests()
    {
        ITestTuples testTuples = testEnv.Env.TestTuples();
        SingleTupleValue(testTuples);
        TwoTupleValues(testTuples);
        ThreeTupleValues(testTuples);
        FourTupleValues(testTuples);
        FiveTupleValues(testTuples);
        SixTupleValues(testTuples);
        SevenTupleValues(testTuples);
        EightTupleValues(testTuples);
        NineTupleValues(testTuples);
        TenTupleValues(testTuples);
        ElevenTupleValues(testTuples);
        TwelveTupleValues(testTuples);
        ThirteenTupleValues(testTuples);
        FourteenTupleValues(testTuples);
        FifteenTupleValues(testTuples);
        SixteenTupleValues(testTuples);
        SeventeenTupleValues(testTuples);
    }

    private static void SingleTupleValue(ITestTuples testTuples)
    {
        var x = testTuples.Tuple_(ValueTuple.Create("a"));

        Assert.Equal("a", x.Item1);
    }

    private static void TwoTupleValues(ITestTuples testTuples)
    {
        (string a, string b) = testTuples.Tuple_(("a", "b"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
    }

    private static void ThreeTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c) = testTuples.Tuple_(("a", "b", "c"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
    }

    private static void FourTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d) = testTuples.Tuple_(("a", "b", "c", "d"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
    }

    private static void FiveTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e) = testTuples.Tuple_(("a", "b", "c", "d", "e"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
    }

    private static void SixTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f) = testTuples.Tuple_(("a", "b", "c", "d", "e", "f"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
    }

    private static void SevenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g) = testTuples.Tuple_(("a", "b", "c", "d", "e", "f", "g"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
    }

    private static void EightTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h) = testTuples.Tuple_(("a", "b", "c", "d", "e", "f", "g", "h"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
    }

    private static void NineTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i) = testTuples.Tuple_(("a", "b", "c", "d", "e", "f", "g", "h", "i"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
    }

    private static void TenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j) = testTuples.Tuple10(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
    }

    private static void ElevenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k) = testTuples.Tuple11(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
        Assert.Equal("k", k);
    }

    private static void TwelveTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l) = testTuples.Tuple12(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
        Assert.Equal("k", k);
        Assert.Equal("l", l);
    }

    private static void ThirteenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m) = testTuples.Tuple13(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
        Assert.Equal("k", k);
        Assert.Equal("l", l);
        Assert.Equal("m", m);
    }

    private static void FourteenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n) = testTuples.Tuple14(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
        Assert.Equal("k", k);
        Assert.Equal("l", l);
        Assert.Equal("m", m);
        Assert.Equal("n", n);
    }

    private static void FifteenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n, string o) = testTuples.Tuple15(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
        Assert.Equal("k", k);
        Assert.Equal("l", l);
        Assert.Equal("m", m);
        Assert.Equal("n", n);
        Assert.Equal("o", o);
    }

    private static void SixteenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n, string o, string p) = testTuples.Tuple16(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
        Assert.Equal("k", k);
        Assert.Equal("l", l);
        Assert.Equal("m", m);
        Assert.Equal("n", n);
        Assert.Equal("o", o);
        Assert.Equal("p", p);
    }

    private static void SeventeenTupleValues(ITestTuples testTuples)
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n, string o, string p, string q) = testTuples.Tuple17(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
        Assert.Equal("i", i);
        Assert.Equal("j", j);
        Assert.Equal("k", k);
        Assert.Equal("l", l);
        Assert.Equal("m", m);
        Assert.Equal("n", n);
        Assert.Equal("o", o);
        Assert.Equal("p", p);
        Assert.Equal("q", q);
    }
}