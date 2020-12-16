using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PrimaryConstructor {

    [Generator]
    public class PrimaryConstructorGenerator : ISourceGenerator {
        private static string AttributeName => "PrimaryConstructorAttribute";
        private static string AttributeText => $@"
using System;
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class PrimaryConstructorAttribute : Attribute {{}}
";

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            var classSymbols = GetClassSymbols(context, receiver);

            foreach (var item in classSymbols) {
                context.AddSource($"{item.Name}.PrimaryConstructor.g.cs",
                    SourceText.From(CreatePrimaryConstructor(item), Encoding.UTF8));
            }

            // var p = SourceText.From(AttributeText, Encoding.UTF8);
            // context.AddSource($"PrimaryConstructorAttribute.g.cs", p);
        }

        private static bool HasInitializer(IFieldSymbol symbol) {
            var field = symbol.DeclaringSyntaxReferences.ElementAtOrDefault(0)?.GetSyntax() as VariableDeclaratorSyntax;
            return field?.Initializer != null;
        }

        private string CreatePrimaryConstructor(INamedTypeSymbol classSymbol) {
            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            var readonlyFields = classSymbol.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName)
                .Where(x => x.IsReadOnly)
                .Where(x => !x.IsStatic)
                .Where(x => !HasInitializer(x))
                .Select(it => new { Type = it.Type.ToDisplayString(), ParameterName = ToCamelCase(it.Name), it.Name })
                .ToList();

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
            var compilation = context.Compilation;
            var classSymbols = new List<INamedTypeSymbol>();
            foreach (var item in receiver.CandidateClasses) {
                var model = compilation.GetSemanticModel(item.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(item)!;
                if (classSymbol.GetAttributes().Any(x => x.AttributeClass.Name == AttributeName)) {
                    classSymbols.Add(classSymbol);
                }
            }
            return classSymbols;
        }
    }
}
