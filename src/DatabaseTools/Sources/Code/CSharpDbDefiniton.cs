using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using DatabaseTools.Model;
using System.Reflection;

namespace DatabaseTools.Sources.Code
{
    internal class CSharpDbDefiniton : IDb
    {
        private Type type;

        public CSharpDbDefiniton(Type type)
        {
            this.type = type;
        }

        public IEnumerable<Table> TableTypes {
            get {
                return this
                    .type
                    .GetProperties()
                    .Where(t => typeof(ITable<>).ImplementedBy(t.PropertyType))
                    .Select(t => {
                        var table = new Table {
                            Name = t.Name,
                            
                            Fields = t
                                .PropertyType
                                .GenericTypeArguments
                                .Single()
                                .GetProperties()
                                .Select(p => new Field {
                                    Name = p.Name,
                                    Type = p.GetCustomAttributes().Any(a => a is DbIgnoreAttribute) ? null : findType(p.PropertyType.Name),
                                    Ignored = p.GetCustomAttributes().Any(a => a is DbIgnoreAttribute)
                                })
                        };

                        var pkAttribute = (PrimaryKeyAttribute) t.PropertyType.GenericTypeArguments.Single().GetTypeInfo().GetCustomAttribute(typeof(PrimaryKeyAttribute));
                        if ( pkAttribute != null ) 
                        {
                            table.PrimaryKey = new Index {
                                Name = "", // TODO
                                Fields = table.Fields.Where(f => pkAttribute.Columns.Contains(f.Name) )
                            };
                        }
                        return table;
                    });
            }
        }

        private string findType(string type)
        {
            var matchedMapping = Program.CSharpMappings.FirstOrDefault(m => string.Equals(m.Item1, type, StringComparison.OrdinalIgnoreCase));

            if ( matchedMapping == null ) 
            {
                throw new Exception();
            }
            return matchedMapping.Item2;
        }

        public void Apply(DbDiff diff)
        {
            throw new NotImplementedException();
        }

        public string GenerateScript(DbDiff diff)
        {
            throw new NotImplementedException();
        }
    }
    
    public static class TypeExtensions
    {    
        public static bool ImplementedBy(this Type generic, Type toCheck) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.GenericTypeArguments.Any() ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.GetInterfaces().FirstOrDefault();
            }
            return false;
        }
    }
}