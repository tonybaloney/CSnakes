using System;
using System.Collections.Generic;
using System.Text;

namespace PythonSourceGenerator.Parser;
public static class PythonSignatureParser
{
    public static bool IsFunctionSignature(string line)
    {
        // Check if the line starts with "def"
        return line.StartsWith("def ") || line.StartsWith("async def");
    }

    public static bool TryParse(string signature, out object? pythonSignature)
    {
        pythonSignature = null;

        // Go line by line
        var lines = signature.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (IsFunctionSignature(line))
            {
                // Parse the function signature
                var lineTokens = PythonSignatureTokenizer.Tokenize(line);
                // Is the last token a colon?
                //if (lineTokens.ConsumeToken().Value != PythonSignatureTokens.PythonSignatureToken.Colon)
                //{
                //    pythonSignature = null;
                //    return false;
                //}
            }
        }
        return false;
    }
}
