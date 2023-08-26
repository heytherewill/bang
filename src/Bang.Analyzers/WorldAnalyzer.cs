using System.Collections.Immutable;
using Bang.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bang.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WorldAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MarkUniqueComponentAsUnique = new(
        id: Diagnostics.World.MarkUniqueComponentAsUnique.Id,
        title: nameof(WorldAnalyzer) + "." + nameof(MarkUniqueComponentAsUnique),
        messageFormat: Diagnostics.World.MarkUniqueComponentAsUnique.Message,
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "When retrieving components using GetUniqueEntity, consider marking that entity with the [Unique]."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(MarkUniqueComponentAsUnique);

    public override void Initialize(AnalysisContext context)
    {
        var syntaxKind = ImmutableArray.Create(
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration
        );

        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(Analyze, syntaxKind);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        // Bail if ISystem is not resolvable.
        var bangSystemInterface = context.Compilation.GetTypeByMetadataName(TypeMetadataNames.SystemInterface);
        if (bangSystemInterface is null)
            return;

        // Bail if the node we are checking is not a type declaration.
        if (context.ContainingSymbol is not INamedTypeSymbol typeSymbol)
            return;

        // Bail if the current type declaration is not a system.
        var isSystem = typeSymbol.ImplementsInterface(bangSystemInterface);
        if (!isSystem)
            return;
    }
}