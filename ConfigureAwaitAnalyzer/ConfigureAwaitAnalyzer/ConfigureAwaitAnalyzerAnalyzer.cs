using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConfigureAwaitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigureAwaitAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ConfigureAwaitAnalyzer";

        private static readonly LocalizableString MissingConfigureTitle = new LocalizableResourceString(nameof(Resources.MissingConfigureTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingConfigureAwaitMessageFormat = new LocalizableResourceString(nameof(Resources.MissingConfigureAwaitMessageFormat), Resources.ResourceManager, typeof(Resources));
        private const string Category = "ConfigureAwaitAnalyzer";

        private static DiagnosticDescriptor MissingConfigureAwaitRule = new DiagnosticDescriptor(DiagnosticId, MissingConfigureTitle, MissingConfigureAwaitMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, customTags: "MissingConfigureAwait");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MissingConfigureAwaitRule);

        public override void Initialize(AnalysisContext context)
            => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AwaitExpression);

        private void AnalyzeNode(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}
