using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigureAwaitAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConfigureAwaitAnalyzerCodeFixProvider)), Shared]
    public class ConfigureAwaitAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string configureAwaitMissingTitle = "Add 'ConfigureAwait(false)'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ConfigureAwaitAnalyzerAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var awaitExpression = root.FindToken(diagnosticSpan.Start).Parent as AwaitExpressionSyntax;

            var tag = diagnostic.Descriptor.CustomTags.First();
            switch (tag)
            {
                case "missing":
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: configureAwaitMissingTitle,
                            createChangedSolution: c => Task.Run(() => AddConfigureAwaitFalse(context.Document, root, awaitExpression, c)),
                            equivalenceKey: configureAwaitMissingTitle),
                        diagnostic);
                    break;
            }
        }

        private Solution AddConfigureAwaitFalse(Document document, SyntaxNode root, AwaitExpressionSyntax awaitExpression, CancellationToken c)
        {
            var configureAwait = SyntaxFactory.IdentifierName(nameof(Task.ConfigureAwait));
            var dotToken = SyntaxFactory.Token(SyntaxKind.DotToken);
            var simpleMemberAccessExpr = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, awaitExpression.Expression, dotToken, configureAwait);

            var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { argument }));

            var invocationExpr = SyntaxFactory.InvocationExpression(simpleMemberAccessExpr, argumentList);
            var newAwaitExpr = awaitExpression.WithExpression(invocationExpr);

            return document
                .WithSyntaxRoot(root.ReplaceNode(awaitExpression, newAwaitExpr))
                .Project
                .Solution;
        }
    }
}
