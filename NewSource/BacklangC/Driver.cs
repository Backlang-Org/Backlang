using System.Reflection;
using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using BacklangC.Stages;
using DistIL;
using DistIL.AsmIO;
using Flo;

namespace BacklangC;

public class Driver
{
    public DriverSettings Settings { get; private init; } = new();
    public required Compilation Compilation { get; set; }
    public required TypeDef ProgramType { get; set; }

    public Optimizer Optimizer { get; set; }
    public List<Message> Messages { get; set; } = [];
    public List<CompilationUnit> Trees { get; set; } = [];

    public static Driver Create(DriverSettings settings)
    {
        var moduleResolver = new ModuleResolver();
        moduleResolver.AddTrustedSearchPaths();

        var module = moduleResolver.Create(settings.RootNamespace, Version.Parse(settings.Version));

        var compilation = new Compilation(module, new ConsoleLogger(), new CompilationSettings());
        var optimizer = new Optimizer();
        optimizer.CreatePassManager(compilation);

        var programType = module.CreateType(settings.RootNamespace, "Program", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

        return new Driver
        {
            Compilation = compilation,
            Settings = settings,
            Optimizer = optimizer,
            ProgramType = programType
        };
    }

    public void Compile()
    {
        var hasError = (List<Message> messages) => messages.Any(_ => _.Severity == MessageSeverity.Error);

        var pipeline = Pipeline.Build<Driver, Driver>(
            cfg => {
                cfg.Add<ParsingStage>();
                cfg.Add<SemanticCheckStage>();

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<ExpandMacrosStage>();
                });

                cfg.Add<SaveModuleStage>();

                /*cfg.When(_ => !hasError(_.Messages) && _.Options.OutputTree, _ => {
                    _.Add<EmitTreeStage>();
                });

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<InitStage>();
                });

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<IntermediateStage>();
                });

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<TypeInheritanceStage>();
                });

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<ExpandImplementationStage>();
                });

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<ImplementationStage>();
                });

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<InitEmbeddedResourcesStage>();
                });

                cfg.When(_ => !hasError(_.Messages), _ => {
                    _.Add<CompileTargetStage>();
                });

                cfg.When(_ => _.Messages.Any(), _ => {
                    _.Add<ReportErrorStage>();
                });
                */
            }
        );

        pipeline.Invoke(this);
    }
}