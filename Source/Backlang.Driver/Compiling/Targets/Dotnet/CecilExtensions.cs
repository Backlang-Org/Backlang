using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class CecilExtensions
{
    public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] arguments)
    {
        var reference = new MethodReference(self.Name, self.ReturnType, self.DeclaringType.MakeGenericInstanceType(arguments))
        {
            HasThis = self.HasThis,
            ExplicitThis = self.ExplicitThis,
            CallingConvention = self.CallingConvention
        };

        foreach (var generic_parameter in self.GenericParameters)
        {
            reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));
        }

        foreach (var p in self.Parameters)
        {
            reference.Parameters.Add(p);
        }

        return reference;
    }
}