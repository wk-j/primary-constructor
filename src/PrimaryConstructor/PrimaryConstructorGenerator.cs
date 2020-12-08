using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace PrimaryConstructor {
    [Generator]
    public class PrimaryConstructorGenerator : ISourceGenerator {
        private const string primaryConstructorAttributeText = @"
using System;
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class PrimaryConstructorAttribute : Attribute { }
";
        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            InjectPrimaryConstructorAttributes(context);

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            var classSymbols = GetClassSymbols(context, receiver);

            foreach (var item in classSymbols) {
                context.AddSource($"{item.Name}.PrimaryConstructor.g.cs", SourceText.From(CreatePrimaryConstructor(item), Encoding.UTF8));
            }
        }

        private string CreatePrimaryConstructor(INamedTypeSymbol classSymbol) {
            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            var readonlyFields = classSymbol.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsReadOnly)
                .Select(it => new { Type = it.Type.ToDisplayString(), ParameterName = ToCamelCase(it.Name), it.Name })
                .ToList();

            // var readonlyBaseFields = classSymbol.BaseType.GetMembers().OfType<IPropertySymbol>()
            //     .Where(x => x.CanBeReferencedByName && x.IsReadOnly)
            //     .Select(it => new { Type = it.Type.ToDisplayString(), ParameterName = ToCamelCase(it.Name), Name = it.Name })
            //     .ToList();

            var arguments = readonlyFields.Select(it => $"{it.Type} {it.ParameterName}");
            var constructor = string.Join(",", arguments);

            var builder = new StringBuilder();
            builder.AppendLine($"namespace {namespaceName} {{");
            builder.AppendLine($"--partial class {classSymbol.Name} {{");
            builder.AppendLine($"----public {classSymbol.Name} ({constructor}) {{");

            foreach (var item in readonlyFields) {
                builder.AppendLine($"------this.{item.Name} = {item.ParameterName};");
            }

            builder.AppendLine("----}");
            builder.AppendLine("--}");
            builder.AppendLine("}");

            return builder.ToString().Replace("--", "  ");
        }

        private static string ToCamelCase(string name) {
            name = name.TrimStart('_');
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }

        private static List<INamedTypeSymbol> GetClassSymbols(GeneratorExecutionContext context, SyntaxReceiver receiver) {
            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var tree = CSharpSyntaxTree.ParseText(SourceText.From(primaryConstructorAttributeText, Encoding.UTF8), options);
            var compilation = context.Compilation.AddSyntaxTrees(tree);

            var attributeSymbol = compilation.GetTypeByMetadataName("PrimaryConstructorAttribute")!;

            var classSymbols = new List<INamedTypeSymbol>();
            foreach (var item in receiver.CandidateClasses) {
                var model = compilation.GetSemanticModel(item.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(item)!;
                if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default))) {
                    classSymbols.Add(classSymbol);
                }
            }

            return classSymbols;
        }

        private static void InjectPrimaryConstructorAttributes(GeneratorExecutionContext context) {
            context.AddSource("PrimaryConstructorAttribute.g.cs",
                SourceText.From(primaryConstructorAttributeText, Encoding.UTF8));
        }
    }
}
