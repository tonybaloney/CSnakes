namespace Integration.Tests;
public class SourceTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestEncoding()
    {
        var module = Env.TestSource();

        var actual = module.GetMadString();

        // Expected string is a mix of different code points and deliberately expressed in an
        // ASCII-safe way to avoid false positive if the same corruption incidentally occurs on both
        // sides.

        const string expected =
            "\u0041" +         // U+0041  (1 UTF-8 byte)  | Letter A                     | A
            "\u00A2" +         // U+00A2  (2 UTF-8 bytes) | Cent Sign                    | ¢
            "\u0939" +         // U+0939  (3 UTF-8 bytes) | Devanagari Letter HA         | ह
            "\uD834\uDD1E" +   // U+1D11E (4 UTF-8 bytes) | Musical Symbol G Clef        | 𝄞
            "\uD83E\uDD8B" +   // U+1F98B (4 UTF-8 bytes) | Butterfly                    | 🦋
            "\uFFFD" +         // U+FFFD                  | Replacement character        | �
            "\u0301" +         // U+0301                  | Combining acute accent       |
            "\u00F1" +         // U+00F1                  | Pre-composed accented letter | ñ
            "\u202E" +         // U+202E                  | Right-to-left override       |
                               // Emoji sequence: white flag + ZWJ + rainbow 🏳️‍🌈
            "\uD83C\uDFF3" +   // U+1F3F3                 | White flag                   | 🏳
            "\uFE0F" +         // U+FE0F                  | Variation selector-16        |
            "\u200D" +         // U+200D                  | Zero-width joiner            |
            "\uD83C\uDF08";    // U+1F308                 | Rainbow                      | 🌈

        Assert.Equal(expected, actual);
    }
}
