using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DatabaseTools
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
                    .Select(t => new Table {
                        Name = t.Name,
                        Fields = t
                            .PropertyType
                            .GenericTypeArguments
                            .Single()
                            .GetProperties()
                            .Select(p => new Field {
                                Name = p.Name,
                                Type = p.PropertyType.Name,
                                Ignored = p.GetCustomAttributes().Any(a => a is DbIgnoreAttribute)
                            })
                    });
            }
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