namespace CSnakes.Parser;

partial class PythonParser
{
    static PythonParser()
    {
        ConstantValueParser = CreateConstantValueParser();
        PythonTypeDefinitionParser = CreatePythonTypeDefinitionParser();
        PythonParameterParser = CreatePythonParameterParser();
        OptionalPythonParameterParser = CreateOptionalPythonParameterParser();
        PythonParameterListParser = CreatePythonParameterListParser();
        PythonFunctionDefinitionParser = CreatePythonFunctionDefinitionParser();
    }
}
