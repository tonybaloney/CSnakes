
namespace PythonSourceGenerator.Parser.Types;

public class PythonFunctionDefinition
{
    PythonTypeSpec _returnType;
    PythonFunctionParameter[] _parameters;

    private void FixupArguments()
    {
        if (_parameters == null || _parameters.Length == 0)
            return;

        // Go through all parameters and mark those after the *arg as keyword only
        bool keywordOnly = false;
        for (int i = 1; i < _parameters.Length; i++)
        {
            if (_parameters[i].ParameterType == PythonFunctionParameterType.Star)
            {
                keywordOnly = true;
                continue;
            }

            _parameters[i].IsKeywordOnly = keywordOnly;
        }
    }

    public string Name { get; set; }

    public PythonFunctionParameter[] Parameters { 
        get { return _parameters; }
        set { _parameters = value; FixupArguments(); }
    }

    public PythonTypeSpec ReturnType
    {
        get { return _returnType ?? new PythonTypeSpec { Name = "Any" }; }
        set => _returnType = value;
    }

    public bool HasReturnTypeAnnotation()
    {
        return _returnType != null;
    }

    public bool IsAsync { get; set; }
}
