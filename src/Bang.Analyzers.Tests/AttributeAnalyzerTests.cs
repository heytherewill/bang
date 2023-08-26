using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bang.Analyzers.Tests;

using Verify = BangAnalyzerVerifier<AttributeAnalyzer>;

[TestClass]
public sealed class AttributeAnalyzerTests
{
    [TestMethod(displayName: "Correctly annotated systems do not trigger the analyzer.")]
    public async Task CorrectlyAnnotatedSystemsDoNotTriggerTheAnalyzer()
    {
        const string source = @"
using Bang.Components;
using Bang.Contexts;
using Bang.Systems;

namespace BangAnalyzerTestNamespace;

public readonly record struct CorrectComponent : IComponent;

[Filter(ContextAccessorKind.Read, typeof(CorrectComponent))]
public sealed class CorrectSystem : ISystem { }";
        await Verify.VerifyAnalyzerAsync(source);
    }

    [TestMethod(displayName: "Filter attributes containing non components trigger one message per wrong type.")]
    public async Task ISystemWithNonComponentsOnFilter()
    {
        const string source = @"
using Bang.Components;
using Bang.Contexts;
using Bang.Systems;

namespace BangAnalyzerTestNamespace;

public readonly record struct Message : IMessage;
public readonly record struct Component : IComponent;
public readonly record struct SomeRandomType;

[Filter(ContextAccessorKind.Read, typeof(Message), typeof(Component), typeof(SomeRandomType))]
public sealed class CorrectSystem : ISystem { }";

        var expectedDiagnostics = new[]
        {
            Verify
                .Diagnostic(AttributeAnalyzer.NonComponentsOnFilterAttribute)
                .WithSpan(12, 35, 12, 50),
            Verify
                .Diagnostic(AttributeAnalyzer.NonComponentsOnFilterAttribute)
                .WithSpan(12, 70, 12, 93),
        };
        await Verify.VerifyAnalyzerAsync(source, expectedDiagnostics);
    }

    [TestMethod(displayName: "Filter attributes containing non components trigger one message per wrong type.")]
    public async Task IMessagerSystemWithNonMessagesOnFilter()
    {
        const string source = @"
using Bang.Components;
using Bang.Contexts;
using Bang.Systems;

namespace BangAnalyzerTestNamespace;

public readonly record struct Message : IMessage;
public readonly record struct Component : IComponent;
public readonly record struct SomeRandomType;

[Messager(typeof(Message))]
[Filter(ContextAccessorKind.Read, typeof(Message), typeof(Component), typeof(SomeRandomType))]
public sealed class CorrectSystem : IMessagerSystem
{
    public void OnMessage(World world, Entity entity, IMessage message) { }
}";
        var expectedDiagnostics = new[]
        {
            Verify
                .Diagnostic(AttributeAnalyzer.NonComponentsOnFilterAttribute)
                .WithSpan(12, 35, 12, 50),
            Verify
                .Diagnostic(AttributeAnalyzer.NonComponentsOnFilterAttribute)
                .WithSpan(12, 70, 12, 93),
        };
        await Verify.VerifyAnalyzerAsync(source);
    }
}