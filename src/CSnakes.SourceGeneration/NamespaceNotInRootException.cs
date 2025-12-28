namespace CSnakes.SourceGeneration;
internal class NamespaceNotInRootException(string path, string rootDir) : ArgumentException($"The file path '{path}' does not match the configured root directory '{rootDir}'.")
{
}
