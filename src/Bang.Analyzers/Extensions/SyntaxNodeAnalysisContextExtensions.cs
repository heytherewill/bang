using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bang.Analyzers.Extensions;

public static class SyntaxNodeAnalysisContextExtensions
{
    public static void ReportDiagnosticIfLackingAttribute(
        this SyntaxNodeAnalysisContext context,
        INamedTypeSymbol type,
        INamedTypeSymbol? attributeToCheck,
        DiagnosticDescriptor diagnosticDescriptor
    )
    {
        var hasAttribute = type.GetAttributes().Any(
            attr => attr.AttributeClass is not null && attr.AttributeClass.Equals(attributeToCheck, SymbolEqualityComparer.IncludeNullability));

        context.ConditionallyReportDiagnostic(diagnosticDescriptor, !hasAttribute);
    }

    public static void ReportDiagnosticIfAttributeExists(
        this SyntaxNodeAnalysisContext context,
        INamedTypeSymbol type,
        INamedTypeSymbol? attributeToCheck,
        DiagnosticDescriptor diagnosticDescriptor
    )
    {
        var hasAttribute = type.GetAttributes().Any(
            attr => attr.AttributeClass is not null && attr.AttributeClass.Equals(attributeToCheck, SymbolEqualityComparer.IncludeNullability));

        context.ConditionallyReportDiagnostic(diagnosticDescriptor, hasAttribute);
    }

    private static void ConditionallyReportDiagnostic(
        this SyntaxNodeAnalysisContext context,
        DiagnosticDescriptor diagnosticDescriptor,
        bool condition
    )
    {
        if (!condition)
            return;

        context.ReportDiagnostic(
            Diagnostic.Create(
                diagnosticDescriptor,
                context.Node.GetLocation()
            )
        );
    }

    public static AttributeData? GetAttributeDataForArgumentList(
        this SyntaxNodeAnalysisContext context,
        AttributeArgumentListSyntax argumentListSyntax
    )
    {
        var attributeSyntax = argumentListSyntax.Parent as AttributeSyntax;
        // First call to .Parent gets the AttributeList.
        // Second call to .Parent get the type annotated with the attribute we're looking for.
        var annotatedTypeNode = attributeSyntax?.Parent?.Parent;
        if (annotatedTypeNode is null)
            return null;

        return context.SemanticModel
            .GetDeclaredSymbol(annotatedTypeNode)?
            .GetAttributes()
            .Single(a => a.ApplicationSyntaxReference!.GetSyntax() == attributeSyntax);
    }

}