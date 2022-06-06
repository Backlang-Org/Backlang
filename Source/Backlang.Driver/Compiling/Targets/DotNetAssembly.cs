using Furesoft.Core.CodeDom.Backends.CLR.Emit;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;

namespace Backlang.Driver.Compiling.Targets;

public class DotNetAssembly : ITargetAssembly
{
    private readonly IAssembly _assembly;
    private readonly AssemblyContentDescription _description;
    private readonly TypeEnvironment _environment;
    private AssemblyDefinition _assemblyDefinition;

    public DotNetAssembly(AssemblyContentDescription description)
    {
        _assembly = description.Assembly;

        var name = new AssemblyNameDefinition(_assembly.FullName.ToString(),
            new Version(1, 0));

        _assemblyDefinition = AssemblyDefinition.CreateAssembly(name, "Module", ModuleKind.Console);
        _description = description;
        this._environment = description.Environment;
    }

    public void WriteTo(Stream output)
    {
        foreach (var type in _assembly.Types)
        {
            var clrType = new TypeDefinition(type.FullName.Qualifier.ToString(),
                type.Name.ToString(), TypeAttributes.Class | TypeAttributes.Public);

            foreach (DescribedBodyMethod m in type.Methods)
            {
                var clrMethod = new MethodDefinition(m.Name.ToString(),
                    MethodAttributes.Public | MethodAttributes.Static,
                    _assemblyDefinition.MainModule.ImportReference(typeof(void)));

                if (m == _description.EntryPoint)
                {
                    _assemblyDefinition.EntryPoint = clrMethod;
                }

                MethodDefinition body = new(m.FullName.ToString(), clrMethod.Attributes, clrMethod.ReturnType);
                clrMethod.Body = new Mono.Cecil.Cil.MethodBody(body);

                clrType.Methods.Add(clrMethod);
            }

            _assemblyDefinition.MainModule.Types.Add(clrType);
        }

        _assemblyDefinition.Write(output);
    }
}