using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConfigureAwaitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigureAwaitAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ConfigureAwaitAnalyzer";

        private static readonly LocalizableString MissingConfigureTitle = new LocalizableResourceString(nameof(Resources.MissingConfigureTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingConfigureAwaitMessage = new LocalizableResourceString(nameof(Resources.MissingConfigureAwaitMessage), Resources.ResourceManager, typeof(Resources));
        private const string Category = "ConfigureAwaitAnalyzer";

        private readonly static DiagnosticDescriptor MissingConfigureAwaitRule = new DiagnosticDescriptor(DiagnosticId, MissingConfigureTitle, MissingConfigureAwaitMessage, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: "missing");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MissingConfigureAwaitRule);

        public override void Initialize(AnalysisContext context)
            => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AwaitExpression);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var awaitExpression = (AwaitExpressionSyntax)context.Node;

            var containsConfigureAwait = awaitExpression
                .DescendantTokens()
                .OfType<SyntaxToken>()
                .Any(x => x.Value is nameof(Task.ConfigureAwait));

            if (!containsConfigureAwait)
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingConfigureAwaitRule, awaitExpression.GetLocation()));
                return;
            }
        }
    }
}
