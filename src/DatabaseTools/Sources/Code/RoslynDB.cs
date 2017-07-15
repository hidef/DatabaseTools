using System;
using DatabaseTools.Model;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DatabaseTools.Sources.Code
{
    public class CodeFile 
    {
        private readonly CompilationUnitSyntax _root;

        public CodeFile(string cSharp)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(cSharp);
            _root = (CompilationUnitSyntax)tree.GetRoot();
        }

        public string Namespace => _root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .Select(x => x.Name.ToString())
                .SingleOrDefault();

        public string[] Usings => _root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(x => x.Name.ToString())
                .ToArray();

        public CodeClass[] Classes => _root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(u => new CodeClass(u, this))
                .ToArray();
    }

    public class CodeClass
    {
        private ClassDeclarationSyntax u;
        private CodeFile file;

        public CodeClass(ClassDeclarationSyntax u, CodeFile file)
        {
            this.u = u;
            this.file = file;
        }

        public string Name => u.Identifier.Text;
        public CodeProperty[] Properties => u.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Select(u => new CodeProperty(u, this))
                .ToArray();
        public CodeAttribute[] Attributes => u.DescendantNodes()
            .OfType<AttributeSyntax>()
            .Select(x => new CodeAttribute( 
                x.Name.ToString(),
                x.ArgumentList?.Arguments.Select((AttributeArgumentSyntax y) => y.ToString().Trim('\"'))
            ))
            .ToArray();

        public CodeFile File => file;
    }

    public class CodeAttribute
    {
        private string name;
        private IEnumerable<string> arguments;

        public CodeAttribute(string name, IEnumerable<string> arguments)
        {
            this.name = name;
            this.arguments = arguments;
        }

        public IEnumerable<string> Arguments => arguments;
        public string Name => name;
    }

    public class CodeProperty
    {
        private PropertyDeclarationSyntax u;
        private CodeClass codeClass;

        public CodeProperty(PropertyDeclarationSyntax u, CodeClass codeClass)
        {
            this.u = u;
            this.codeClass = codeClass;
        }

        public string Name => u.Identifier.Text;
        public string Type => u.Type.ToString();
        public bool IsIgnored => false;
        public CodeClass Class => this.codeClass;

        public CodeAttribute[] Attributes => u.DescendantNodes()
            .OfType<AttributeSyntax>()
            .Select(x => new CodeAttribute( 
                x.Name.ToString(),
                x.ArgumentList?.Arguments.Select((AttributeArgumentSyntax y) => y.ToString().Trim('\"'))
            ))
            .ToArray();
    }

    public class RoslynDB : IDb
    {
        private readonly string _databaseFilePath;
        private readonly IEnumerable<CodeFile> _codeBase;

        public RoslynDB(string databaseFilePath) 
        {
            _databaseFilePath = new Uri(databaseFilePath).LocalPath;
            _codeBase = Directory.GetFiles(_databaseFilePath, "*.cs", SearchOption.AllDirectories)
                .Select(
                    fn => new CodeFile(File.ReadAllText(fn))
                );
        }

        public void Apply(DbDiff diff)
        {
            throw new NotImplementedException();
        }

        public string GenerateScript(DbDiff diff)
        {
            throw new NotImplementedException();
        }

        public DatabaseModel GetModel()
        {
            CodeClass databaseSchema = _codeBase.SelectMany(x => x.Classes).Single(x => x.Name == "Database");

            var model = new DatabaseModel {
                Tables = databaseSchema
                .Properties
                .Where(x => x.Type.StartsWith("ITable<"))
                .Select(x => new Table {
                    Name = x.Name,
                    PrimaryKey = resolveClass(x.Type.Replace("ITable<", "").Replace(">", ""), x.Class.File.Namespace, x.Class.File.Usings)
                        .Attributes.SingleOrDefault(y => y.Name == "PrimaryKey").Arguments.ToArray(),
                    Fields = resolveClass(x.Type.Replace("ITable<", "").Replace(">", ""), x.Class.File.Namespace, x.Class.File.Usings)
                        .Properties
                        .Select(y => new Field {
                            Name = y.Name,
                            Type = y.Type,
                            Ignored = y.Attributes.Any(z => z.Name == "DbIgnore")
                        })
                        .ToList()
                })
                .ToList()
            };

            return model;
        }

        private CodeClass resolveClass(string type, string currentNamespace, string[] usings)
        {
            CodeClass localClass = this._codeBase
                .SelectMany(x => x.Classes)
                .SingleOrDefault(x => x.File.Namespace == currentNamespace && x.Name == type);

            if ( localClass != null ) return localClass;

            CodeClass remoteClass = 
                _codeBase
                .Where(x => usings.Contains(x.Namespace))
                .SelectMany(x => x.Classes)
                .Single(x => x.Name == type);

            return remoteClass;
        }

        public object resolveTypeInfo(string name, List<string> references)
        {
            Console.WriteLine(name);
            references.ForEach(x => Console.WriteLine(x));
            return new {
                Name = name
            };
        }
    }
}