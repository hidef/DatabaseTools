using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using DatabaseTools.Model;

namespace DatabaseTools.Sources.Code
{
    public class CSharpDbDefiniton : IDb
    {
        private Type type;

        public CSharpDbDefiniton(Type type)
        {
            this.type = type;
        }

        public IList<Table> TableTypes {
            get {
                return this
                    .type
                    .GetProperties()
                    .Where(t => typeof(ITable<>).ImplementedBy(t.PropertyType))
                    .Select(t => {
                        var foreignKeys = t
                                .PropertyType
                                .GenericTypeArguments
                                .Single()
                                .GetProperties()
                                .Select(getForeignKey)
                                .Where(x => x != null)
                                .ToList();
                                

                        var table = new Table {
                            Name = t.Name,
                            
                            Fields = t
                                .PropertyType
                                .GenericTypeArguments
                                .Single()
                                .GetProperties()
                                .Select(p => {
                                    var f = new Field {
                                        Name = p.Name,
                                        Type = p.GetCustomAttributes().Any(a => a is DbIgnoreAttribute) ? null : findType(p.PropertyType.Name),
                                        Ignored = p.GetCustomAttributes().Any(a => a is DbIgnoreAttribute)
                                    };

                                    // // if foreign key, create the column as the type of the referenced column
                                    // if ( f.Type == null ) 
                                    // {
                                    //     var fk = foreignKeys.SingleOrDefault(k => k.LocalColumn == p.Name);
                                    //     if ( fk != null) 
                                    //     {
                                    //         Console.WriteLine($"fk.Name {fk.Name}");
                                    //         Console.WriteLine($"fk.RemoteTable {fk.RemoteTable}");
                                    //         Console.WriteLine($"fk.RemoteColumn {fk.RemoteColumn}");
                                    //         string fkType = p.PropertyType.GetField(fk.LocalColumn).FieldType.Name;
                                    //         f.Type = findType(fkType);
                                    //     }
                                    // }
                                    return f;
                                })
                                .ToList()
                        };

                        var pkAttribute = (PrimaryKeyAttribute) t.PropertyType.GenericTypeArguments.Single().GetTypeInfo().GetCustomAttribute(typeof(PrimaryKeyAttribute));
                        if ( pkAttribute != null ) 
                        {
                            table.PrimaryKey = pkAttribute.Columns;
                        }
                        return table;
                    })
                    .ToList();
            }
        }

        private ForeignKey getForeignKey(PropertyInfo p)
        {
            ForeignKeyAttribute fkAttribute = (ForeignKeyAttribute) p.GetCustomAttribute(typeof(ForeignKeyAttribute));
            
            if ( fkAttribute == null ) return null;

            var foreignType = p.PropertyType;
            string foreignTableName = foreignType.Name;

            ForeignKey fk = new ForeignKey {
                Name = $"FK_{p.Name}_{foreignTableName}_{fkAttribute.Column}",
                LocalColumn =  p.Name,
                RemoteColumn = fkAttribute.Column,
                RemoteTable = foreignTableName
            };

            return fk;
        }

        private string findType(string type)
        {
            var matchedMapping = TypeMappings.CSharpMappings.FirstOrDefault(m => string.Equals(m.Item1, type, StringComparison.OrdinalIgnoreCase));

            return matchedMapping?.Item2;
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
            return new DatabaseModel {
                Tables = this.TableTypes
            };
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