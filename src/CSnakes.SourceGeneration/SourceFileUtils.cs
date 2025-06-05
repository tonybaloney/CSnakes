using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CSnakes.SourceGeneration;
internal static class SourceFileUtils
{
    internal static string ToBaseUTF864(this SourceText text)
    {
        var bytes = Encoding.UTF8.GetBytes(text.ToString());
        return Convert.ToBase64String(bytes);
    }
}
