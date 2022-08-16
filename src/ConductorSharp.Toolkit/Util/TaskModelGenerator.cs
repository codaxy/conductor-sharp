﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace ConductorSharp.Toolkit.Util
{
    public class TaskModelGenerator
    {
        public enum ModelType
        {
            Task,
            Workflow
        };

        public class PropertyData
        {
            public string Name { get; set; }
            public string OriginalName { get; set; }
            public string Type { get; set; }
            public Dictionary<string, string> XmlComments { get; } = new();
        }

        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string OriginalName { get; set; }
        public string Summary { get; set; }

        protected CompilationUnitSyntax _compilationUnit = SyntaxFactory.CompilationUnit();
        private readonly ModelType _modelType;
        private List<PropertyData> _inputProperties = new();
        private List<PropertyData> _outputProperties = new();
        private readonly Dictionary<string, string> _xmlComments = new();

        public TaskModelGenerator(string @namespace, string className, ModelType modelType)
        {
            Namespace = @namespace;
            ClassName = className;
            _modelType = modelType;
        }

        public void AddInputProperty(PropertyData propData)
        {
            _inputProperties.Add(propData);
        }

        public void AddOutputProperty(PropertyData propData)
        {
            _outputProperties.Add(propData);
        }

        public void AddXmlComment(string tag, string value) => _xmlComments[tag] = value;

        public string Build()
        {
            _compilationUnit = _compilationUnit.AddUsings(
                CreateUsings("ConductorSharp.Engine.Model", "ConductorSharp.Engine.Util", "MediatR", "Newtonsoft.Json")
            );
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(Namespace));
            namespaceDeclaration = namespaceDeclaration.AddMembers(CreateInputClass(), CreateOutputClass(), CreateModelClass());
            _compilationUnit = _compilationUnit.AddMembers(namespaceDeclaration);
            return _compilationUnit.NormalizeWhitespace().ToFullString();
        }

        private SyntaxTriviaList GenerateAutoGeneratedComment()
        {
            // This comment is written:

            // <autogenerated>
            //     This code was generated by a tool.
            //
            //     Changes to this file may cause incorrect behavior and will be lost if
            //     the code is regenerated.
            // </autogenerated>

            var list = SyntaxFactory.TriviaList();
            list = list.Add(SyntaxFactory.Comment("// <autogenerated>"));
            list = list.Add(SyntaxFactory.Comment("//     This code was generated by a tool."));
            list = list.Add(SyntaxFactory.Comment("//     Changes to this file may cause incorrect behavior and will be lost if"));
            list = list.Add(SyntaxFactory.Comment("//     the code is regenerated."));
            list = list.Add(SyntaxFactory.Comment("// </autogenerated>"));

            return list;
        }

        private UsingDirectiveSyntax[] CreateUsings(params string[] namespaces)
        {
            var usings = namespaces.Select(@namespace => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace))).ToArray();
            usings[0] = usings[0].WithLeadingTrivia(GenerateAutoGeneratedComment());
            return usings;
        }

        private AttributeSyntax CreateInputPropertyAttribute(string originalName)
        {
            var attribute = SyntaxFactory
                .Attribute(SyntaxFactory.ParseName("JsonProperty"))
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(originalName))
                    )
                );
            return attribute;
        }

        private PropertyDeclarationSyntax CreateProperty(PropertyData inputData)
        {
            var propertyDeclaration = SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(inputData.Type), inputData.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory
                        .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory
                        .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                );

            var attributeList = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(CreateInputPropertyAttribute(inputData.OriginalName))
            );
            propertyDeclaration = propertyDeclaration.AddAttributeLists(attributeList);
            propertyDeclaration = propertyDeclaration.WithLeadingTrivia(GenerateXmlDocComment(inputData.XmlComments));

            return propertyDeclaration;
        }

        private SyntaxTriviaList GenerateXmlDocComment(Dictionary<string, string> xmlComments)
        {
            var list = new SyntaxTriviaList();

            foreach (var elem in xmlComments)
            {
                list = list.Add(SyntaxFactory.Comment($"/// <{elem.Key}>"));
                list = list.Add(SyntaxFactory.Comment($"/// {elem.Value}"));
                list = list.Add(SyntaxFactory.Comment($"/// </{elem.Key}>"));
            }

            return list;
        }

        private ClassDeclarationSyntax CreateInputClass()
        {
            var typeArgumentList = SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.ParseTypeName($"{ClassName}Output"))
            );
            var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.GenericName("IRequest").WithTypeArgumentList(typeArgumentList));
            var baseList = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType));
            var classDeclaration = SyntaxFactory
                .ClassDeclaration($"{ClassName}Input")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .WithBaseList(baseList)
                .AddMembers(_inputProperties.Select(CreateProperty).ToArray());

            return classDeclaration;
        }

        private ClassDeclarationSyntax CreateOutputClass()
        {
            var classDeclaration = SyntaxFactory
                .ClassDeclaration($"{ClassName}Output")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(_outputProperties.Select(CreateProperty).ToArray());

            return classDeclaration;
        }

        private ClassDeclarationSyntax CreateModelClass()
        {
            var typeArgumentList = SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SeparatedList(
                    new List<TypeSyntax> { SyntaxFactory.ParseTypeName($"{ClassName}Input"), SyntaxFactory.ParseTypeName($"{ClassName}Output") }
                )
            );
            var baseType = SyntaxFactory.SimpleBaseType(
                SyntaxFactory
                    .GenericName(_modelType == ModelType.Task ? "SimpleTaskModel" : "SubWorkflowTaskModel")
                    .WithTypeArgumentList(typeArgumentList)
            );
            var baseList = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType));
            var classDeclaration = SyntaxFactory
                .ClassDeclaration(ClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .WithBaseList(baseList)
                .WithLeadingTrivia(GenerateXmlDocComment(_xmlComments));

            return classDeclaration;
        }
    }
}
