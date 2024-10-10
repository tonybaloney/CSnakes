namespace Integration.Tests;

public class TupleTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    ITestTuples TestTuples => Env.TestTuples();

    [Fact]
    public void SingleTupleValue()
    {
        var x = TestTuples.Tuple_(ValueTuple.Create("a"));

        Assert.Equal("a", x.Item1);
    }

    [Fact]
    public void TwoTupleValues()
    {
        (string a, string b) = TestTuples.Tuple_(("a", "b"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
    }

    [Fact]
    public void ThreeTupleValues()
    {
        (string a, string b, string c) = TestTuples.Tuple_(("a", "b", "c"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
    }

    [Fact]
    public void FourTupleValues()
    {
        (string a, string b, string c, string d) = TestTuples.Tuple_(("a", "b", "c", "d"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
    }

    [Fact]
    public void FiveTupleValues()
    {
        (string a, string b, string c, string d, string e) = TestTuples.Tuple_(("a", "b", "c", "d", "e"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
    }

    [Fact]
    public void SixTupleValues()
    {
        (string a, string b, string c, string d, string e, string f) = TestTuples.Tuple_(("a", "b", "c", "d", "e", "f"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
    }

    [Fact]
    public void SevenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g) = TestTuples.Tuple_(("a", "b", "c", "d", "e", "f", "g"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
    }

    [Fact]
    public void EightTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h) = TestTuples.Tuple_(("a", "b", "c", "d", "e", "f", "g", "h"));
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
        Assert.Equal("d", d);
        Assert.Equal("e", e);
        Assert.Equal("f", f);
        Assert.Equal("g", g);
        Assert.Equal("h", h);
    }

    [Fact]
    public void NineTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i) = TestTuples.Tuple_(("a", "b", "c", "d", "e", "f", "g", "h", "i"));
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

    [Fact]
    public void TenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j) = TestTuples.Tuple10(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j"));
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

    [Fact]
    public void ElevenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k) = TestTuples.Tuple11(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k"));
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

    [Fact]
    public void TwelveTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l) = TestTuples.Tuple12(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l"));
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

    [Fact]
    public void ThirteenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m) = TestTuples.Tuple13(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m"));
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

    [Fact]
    public void FourteenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n) = TestTuples.Tuple14(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n"));
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

    [Fact]
    public void FifteenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n, string o) = TestTuples.Tuple15(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o"));
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

    [Fact]
    public void SixteenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n, string o, string p) = TestTuples.Tuple16(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p"));
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

    [Fact]
    public void SeventeenTupleValues()
    {
        (string a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n, string o, string p, string q) = TestTuples.Tuple17(("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q"));
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