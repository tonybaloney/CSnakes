namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionDefinition(string name, PythonTypeSpec? returnType, PythonFunctionParameter[] pythonFunctionParameter)
{
    public string Name { get; private set; } = name;

    private readonly PythonFunctionParameter[] _parameters = FixupArguments(pythonFunctionParameter);

    private static PythonFunctionParameter[] FixupArguments(PythonFunctionParameter[]? parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return [];

        // Go through all parameters and mark those after the *arg as keyword only
        bool keywordOnly = false;
        for (int i = 1; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType == PythonFunctionParameterType.Star)
            {
                keywordOnly = true;
                continue;
            }

            parameters[i].IsKeywordOnly = keywordOnly;
        }

        return parameters;
    }

    public PythonTypeSpec ReturnType => returnType ?? PythonTypeSpec.Any;

    public PythonFunctionParameter[] Parameters => _parameters;

    public bool HasReturnTypeAnnotation() => returnType is not null;

    public bool IsAsync { get; set; }
}
