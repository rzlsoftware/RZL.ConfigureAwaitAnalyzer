using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigureAwaitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigureAwaitAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ConfigureAwaitAnalyzer";

        private static readonly LocalizableString MissingConfigureAwaitTitle = new LocalizableResourceString(nameof(Resources.MissingConfigureTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingConfigureAwaitMessage = new LocalizableResourceString(nameof(Resources.MissingConfigureAwaitMessage), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ConfigureAwaitTrueTitle = new LocalizableResourceString(nameof(Resources.ConfigureAwaitTrueTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ConfigureAwaitTrueMessage = new LocalizableResourceString(nameof(Resources.ConfigureAwaitTrueMessage), Resources.ResourceManager, typeof(Resources));
        private const string Category = "ConfigureAwaitAnalyzer";

        private readonly static DiagnosticDescriptor MissingConfigureAwaitRule = new DiagnosticDescriptor(DiagnosticId, MissingConfigureAwaitTitle, MissingConfigureAwaitMessage, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: "missing");
        private readonly static DiagnosticDescriptor ConfigureAwaitTrueRule = new DiagnosticDescriptor(DiagnosticId, ConfigureAwaitTrueTitle, ConfigureAwaitTrueMessage, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: "true");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MissingConfigureAwaitRule, ConfigureAwaitTrueRule);

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

            var isConfigureAwaitTrue = awaitExpression
                .DescendantNodes()
                .OfType<ArgumentListSyntax>()
                .Last()
                .Arguments
                .First()
                .GetFirstToken()
                .IsKind(SyntaxKind.TrueKeyword);

            if (isConfigureAwaitTrue)
            {
                context.ReportDiagnostic(Diagnostic.Create(ConfigureAwaitTrueRule, awaitExpression.GetLocation()));
                return;
            }
        }
    }
}
