using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CSnakes.SourceGeneration;
internal static class SourceFileUtils
{
    internal static string ToBase64(this SourceText text)
    {
        Encoding enc = text.Encoding ?? Encoding.UTF8;
        var bytes = enc.GetBytes(text.ToString());
        return Convert.ToBase64String(bytes);
    }
}
