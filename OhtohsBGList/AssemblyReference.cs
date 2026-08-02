using System.Reflection;

namespace OhtohsBGList;

public static class AssemblyReference
{
    private static readonly Assembly _assembly =
        Assembly.GetAssembly(typeof(AssemblyReference))
        ?? throw new InvalidOperationException("Assembly not found");

    public static Assembly Assembly => _assembly;
}