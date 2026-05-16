using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Walkers
{
    /// <summary>
    /// Custom Roslyn syntax walker that extracts semantic chunks from the AST.
    /// </summary>
    public class AstChunkingWalker : CSharpSyntaxWalker
    {
        private readonly string _filePath;
        private readonly string _sourceCode;
        private readonly List<CodeChunk> _chunks;
        private readonly Stack<string> _namespacePath;
        private readonly Stack<string> _classPath;

        public IEnumerable<CodeChunk> Chunks => _chunks;

        public AstChunkingWalker
        (
            string filePath,
            string sourceCode
        )
        {
            _filePath = filePath;
            _sourceCode = sourceCode;
            _chunks = new List<CodeChunk>();
            _namespacePath = new Stack<string>();
            _classPath = new Stack<string>();
        }

        // ============= Namespace Handling =============

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            _namespacePath.Push(node.Name.ToString());
            base.VisitNamespaceDeclaration(node);
            _namespacePath.Pop();
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            _namespacePath.Push(node.Name.ToString());
            base.VisitFileScopedNamespaceDeclaration(node);
            _namespacePath.Pop();
        }

        // ============= Class Handling =============

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            ExtractTypeDeclaration(node, node.Identifier.Text, "InterfaceDeclaration");
            _classPath.Push(node.Identifier.Text);
            base.VisitInterfaceDeclaration(node);
            _classPath.Pop();
        }

        // ============= Struct Handling =============

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            ExtractTypeDeclaration(node, node.Identifier.Text, "StructDeclaration");
            _classPath.Push(node.Identifier.Text);
            base.VisitStructDeclaration(node);
            _classPath.Pop();
        }

        // ============= Enum Handling =============

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            ExtractTypeDeclaration(node, node.Identifier.Text, "EnumDeclaration");
            base.VisitEnumDeclaration(node);
        }

        // ============= Method Handling =============

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            ExtractMethodDeclaration(node);
            base.VisitMethodDeclaration(node);
        }

        // ============= Property Handling =============

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            ExtractMemberDeclaration(node, node.Identifier.Text, "PropertyDeclaration");
            base.VisitPropertyDeclaration(node);
        }

        // ============= Helper Methods =============

        /// <summary>
        /// Extracts a type declaration (class, interface, struct, enum) as a chunk.
        /// </summary>
        private void ExtractTypeDeclaration
        (
            SyntaxNode node,
            string name,
            string nodeType
        )
        {
            var nodeUri = BuildNodeUri(name);
            var (startLine, endLine) = GetLineNumbers(node);
            var content = ExtractNodeContent(node);
            var metadata = ExtractTypeMetadata(node, nodeType);

            var chunk = new CodeChunk(
                Content: content,
                FilePath: _filePath,
                NodeUri: nodeUri,
                NodeType: nodeType,
                StartLine: startLine,
                EndLine: endLine,
                Metadata: metadata
            );

            _chunks.Add(chunk);
        }

        /// <summary>
        /// Extracts a method declaration as a chunk.
        /// </summary>
        private void ExtractMethodDeclaration(MethodDeclarationSyntax node)
        {
            var methodName = node.Identifier.Text;
            var nodeUri = BuildNodeUri(methodName);
            var (startLine, endLine) = GetLineNumbers(node);
            var content = ExtractNodeContent(node);
            var metadata = ExtractMethodMetadata(node);

            var chunk = new CodeChunk(
                Content: content,
                FilePath: _filePath,
                NodeUri: nodeUri,
                NodeType: "MethodDeclaration",
                StartLine: startLine,
                EndLine: endLine,
                Metadata: metadata
            );

            _chunks.Add(chunk);
        }

        /// <summary>
        /// Extracts a member declaration (property, field) as a chunk.
        /// </summary>
        private void ExtractMemberDeclaration
        (
            SyntaxNode node,
            string name,
            string nodeType
        )
        {
            var nodeUri = BuildNodeUri(name);
            var (startLine, endLine) = GetLineNumbers(node);
            var content = ExtractNodeContent(node);
            var metadata = ExtractMemberMetadata(node, nodeType);

            var chunk = new CodeChunk(
                Content: content,
                FilePath: _filePath,
                NodeUri: nodeUri,
                NodeType: nodeType,
                StartLine: startLine,
                EndLine: endLine,
                Metadata: metadata
            );

            _chunks.Add(chunk);
        }

        /// <summary>
        /// Builds a fully qualified node URI from current namespace and class context.
        /// </summary>
        private string BuildNodeUri(string nodeName)
        {
            var parts = new List<string>();

            // Add namespace (in reverse order due to stack)
            var namespaceParts = _namespacePath.Reverse().ToList();
            parts.AddRange(namespaceParts);

            // Add class hierarchy (in reverse order due to stack)
            var classParts = _classPath.Reverse().ToList();
            parts.AddRange(classParts);

            // Add the node name
            parts.Add(nodeName);

            return string.Join(".", parts);
        }

        /// <summary>
        /// Extracts the source code content for a given AST node, preserving trivia.
        /// </summary>
        private string ExtractNodeContent(SyntaxNode node)
        {
            var leadingTrivia = node.GetLeadingTrivia().ToString();
            var nodeText = node.ToString();
            return (leadingTrivia + nodeText).Trim();
        }

        /// <summary>
        /// Gets the starting and ending line numbers for a node.
        /// </summary>
        private (int startLine, int endLine) GetLineNumbers(SyntaxNode node)
        {
            var span = node.GetLocation().GetLineSpan();
            return (
                startLine: span.StartLinePosition.Line + 1, // Convert to 1-based indexing
                endLine: span.EndLinePosition.Line + 1
            );
        }

        /// <summary>
        /// Extracts metadata for type declarations.
        /// </summary>
        private Dictionary<string, string> ExtractTypeMetadata(SyntaxNode node, string nodeType)
        {
            var metadata = new Dictionary<string, string>();

            // Extract access modifiers
            if (node is BaseTypeDeclarationSyntax typeDecl)
            {
                var accessModifier = ExtractAccessModifier(typeDecl.Modifiers);
                if (!string.IsNullOrEmpty(accessModifier))
                {
                    metadata["AccessModifier"] = accessModifier;
                }

                // Add namespace context
                var nsContext = string.Join(".", _namespacePath.Reverse());
                if (!string.IsNullOrEmpty(nsContext))
                {
                    metadata["Namespace"] = nsContext;
                }

                // Check for sealed, abstract, static keywords
                if (typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)))
                {
                    metadata["IsSealed"] = "true";
                }
                if (typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                {
                    metadata["IsAbstract"] = "true";
                }
                if (typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                {
                    metadata["IsStatic"] = "true";
                }
            }

            metadata["NodeType"] = nodeType;
            return metadata;
        }

        /// <summary>
        /// Extracts metadata for method declarations.
        /// </summary>
        private Dictionary<string, string> ExtractMethodMetadata(MethodDeclarationSyntax node)
        {
            var metadata = new Dictionary<string, string>();

            // Access modifier
            var accessModifier = ExtractAccessModifier(node.Modifiers);
            if (!string.IsNullOrEmpty(accessModifier))
            {
                metadata["AccessModifier"] = accessModifier;
            }

            // Return type
            metadata["ReturnType"] = node.ReturnType.ToString();

            // Parent class context
            if (_classPath.Count > 0)
            {
                metadata["ParentClass"] = _classPath.Peek();
            }

            // Namespace context
            var nsContext = string.Join(".", _namespacePath.Reverse());
            if (!string.IsNullOrEmpty(nsContext))
            {
                metadata["Namespace"] = nsContext;
            }

            // Parameter count
            metadata["ParameterCount"] = node.ParameterList.Parameters.Count.ToString();

            // Check for async, static, virtual, override
            if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
            {
                metadata["IsAsync"] = "true";
            }

            if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                metadata["IsStatic"] = "true";
            }

            if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)))
            {
                metadata["IsVirtual"] = "true";
            }

            if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
            {
                metadata["IsOverride"] = "true";
            }

            return metadata;
        }

        /// <summary>
        /// Extracts metadata for member declarations.
        /// </summary>
        private Dictionary<string, string> ExtractMemberMetadata(SyntaxNode node, string nodeType)
        {
            var metadata = new Dictionary<string, string>();

            if (node is PropertyDeclarationSyntax propDecl)
            {
                var accessModifier = ExtractAccessModifier(propDecl.Modifiers);
                if (!string.IsNullOrEmpty(accessModifier))
                {
                    metadata["AccessModifier"] = accessModifier;
                }

                metadata["PropertyType"] = propDecl.Type.ToString();

                // Check for accessors
                if (propDecl.AccessorList != null)
                {
                    var getAccessor = propDecl.AccessorList.Accessors.
                                        FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

                    var setAccessor = propDecl.AccessorList.Accessors.
                                        FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

                    if (getAccessor != null)
                        metadata["HasGetter"] = "true";

                    if (setAccessor != null)
                        metadata["HasSetter"] = "true";
                }

                // Parent class context
                if (_classPath.Count > 0)
                {
                    metadata["ParentClass"] = _classPath.Peek();
                }
            }

            metadata["NodeType"] = nodeType;
            return metadata;
        }

        /// <summary>
        /// Extracts the primary access modifier from a token list.
        /// </summary>
        private static string ExtractAccessModifier(SyntaxTokenList modifiers)
        {
            foreach (var modifier in modifiers)
            {
                return modifier.Kind() switch
                {
                    SyntaxKind.PublicKeyword => "public",
                    SyntaxKind.PrivateKeyword => "private",
                    SyntaxKind.ProtectedKeyword => "protected",
                    SyntaxKind.InternalKeyword => "internal",
                    _ => null!
                };
            }
            return string.Empty;
        }
    }
}