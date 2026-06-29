namespace Jarvis.Core.Agents.Coding.Parsing;

using Jarvis.Core.Agents.Coding.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Parses C# source files using Roslyn.
/// </summary>
public sealed class CSharpParser : LanguageParserBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpParser"/> class.
    /// </summary>
    public CSharpParser()
        : base("C#")
    {
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeSymbol> Parse(RepositoryFile file, string sourceText)
    {
        ArgumentNullException.ThrowIfNull(file);

        var tree = CSharpSyntaxTree.ParseText(sourceText ?? string.Empty, path: file.Path);
        var walker = new SymbolWalker(file, tree, CreateId);
        walker.Visit(tree.GetRoot());
        return walker.Symbols;
    }

    private sealed class SymbolWalker : CSharpSyntaxWalker
    {
        private readonly RepositoryFile file;
        private readonly SyntaxTree tree;
        private readonly Func<RepositoryFile, string, string, int, string> createId;
        private readonly Stack<CodeSymbol> parents = new();
        private readonly List<CodeSymbol> symbols = [];

        public SymbolWalker(
            RepositoryFile file,
            SyntaxTree tree,
            Func<RepositoryFile, string, string, int, string> createId)
        {
            this.file = file;
            this.tree = tree;
            this.createId = createId;
        }

        public IReadOnlyList<CodeSymbol> Symbols => symbols;

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            VisitContainer(CreateSymbol<NamespaceSymbol>(node.Name.ToString(), node), () => base.VisitNamespaceDeclaration(node));
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            VisitContainer(CreateSymbol<NamespaceSymbol>(node.Name.ToString(), node), () => base.VisitFileScopedNamespaceDeclaration(node));
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            VisitContainer(CreateSymbol<ClassSymbol>(node.Identifier.Text, node), () => base.VisitClassDeclaration(node));
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            VisitContainer(CreateSymbol<InterfaceSymbol>(node.Identifier.Text, node), () => base.VisitInterfaceDeclaration(node));
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            VisitContainer(CreateSymbol<EnumSymbol>(node.Identifier.Text, node), () => base.VisitEnumDeclaration(node));
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            VisitContainer(CreateSymbol<StructSymbol>(node.Identifier.Text, node), () => base.VisitStructDeclaration(node));
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            VisitContainer(CreateSymbol<RecordSymbol>(node.Identifier.Text, node), () => base.VisitRecordDeclaration(node));
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AddSymbol(CreateSymbol<MethodSymbol>(node.Identifier.Text, node));
            base.VisitMethodDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            AddSymbol(CreateSymbol<ConstructorSymbol>(node.Identifier.Text, node));
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            AddSymbol(CreateSymbol<PropertySymbol>(node.Identifier.Text, node));
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            foreach (var variable in node.Declaration.Variables)
            {
                AddSymbol(CreateSymbol<FieldSymbol>(variable.Identifier.Text, node));
            }

            base.VisitFieldDeclaration(node);
        }

        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            AddSymbol(CreateSymbol<EventSymbol>(node.Identifier.Text, node));
            base.VisitEventDeclaration(node);
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            foreach (var variable in node.Declaration.Variables)
            {
                AddSymbol(CreateSymbol<EventSymbol>(variable.Identifier.Text, node));
            }

            base.VisitEventFieldDeclaration(node);
        }

        private void VisitContainer(CodeSymbol symbol, Action visitChildren)
        {
            AddSymbol(symbol);
            parents.Push(symbol);
            visitChildren();
            parents.Pop();
        }

        private T CreateSymbol<T>(string name, SyntaxNode node)
            where T : CodeSymbol, new()
        {
            var line = tree.GetLineSpan(node.Span).StartLinePosition.Line + 1;
            var symbol = new T
            {
                Id = createId(file, new T().Kind, name, line),
                Name = name,
                Language = "C#",
                File = file.RelativePath,
                Line = line,
                Parent = parents.TryPeek(out var parent) ? parent.Id : string.Empty,
                Accessibility = GetAccessibility(node),
                Modifiers = GetModifiers(node),
                Attributes = GetAttributes(node),
                Project = GetProjectName(file.RelativePath)
            };

            return symbol;
        }

        private void AddSymbol(CodeSymbol symbol)
        {
            symbols.Add(symbol);
            if (parents.TryPeek(out var parent))
            {
                parent.Children.Add(symbol);
            }
        }

        private static string GetAccessibility(SyntaxNode node)
        {
            var modifiers = GetModifierTexts(node);
            if (modifiers.Contains("public"))
            {
                return "public";
            }

            if (modifiers.Contains("private"))
            {
                return "private";
            }

            if (modifiers.Contains("protected") && modifiers.Contains("internal"))
            {
                return "protected internal";
            }

            if (modifiers.Contains("protected"))
            {
                return "protected";
            }

            if (modifiers.Contains("internal"))
            {
                return "internal";
            }

            return string.Empty;
        }

        private static List<string> GetModifiers(SyntaxNode node)
        {
            return GetModifierTexts(node)
                .Where(modifier => modifier is "static" or "abstract" or "virtual" or "override" or "sealed" or "readonly" or "async")
                .ToList();
        }

        private static List<string> GetModifierTexts(SyntaxNode node)
        {
            return node switch
            {
                MemberDeclarationSyntax member => member.Modifiers.Select(modifier => modifier.Text).ToList(),
                _ => []
            };
        }

        private static List<string> GetAttributes(SyntaxNode node)
        {
            return node switch
            {
                MemberDeclarationSyntax member => member.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Select(attribute => attribute.Name.ToString())
                    .ToList(),
                _ => []
            };
        }

        private static string GetProjectName(string relativePath)
        {
            var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[0] : string.Empty;
        }
    }
}
