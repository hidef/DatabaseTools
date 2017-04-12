using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Text;
using DatabaseTools.Model;

namespace DatabaseTools.Sources.MySQL
{
    internal class SqlDb : IDb
    {
        private string connectionString;
        IDbConnection _connection;

        public SqlDb(Uri uri)
        {
            var connectionStringBuilder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
            connectionStringBuilder.Server = uri.Host;
            connectionStringBuilder.Port = (uint) (uri.Port == -1 ? 3306 : uri.Port);
            connectionStringBuilder.Database = uri.LocalPath.TrimStart('/');
            connectionStringBuilder.UserID = uri.UserInfo.Split(':')[0];
            connectionStringBuilder.Password = uri.UserInfo.Split(':')[1];
            this.connectionString = connectionStringBuilder.GetConnectionString(true);
            
            _connection = new MySql.Data.MySqlClient.MySqlConnection(this.connectionString);
            _connection.Open();   
        }

        public IEnumerable<Table> TableTypes {
            get { 
                string databaseName = _connection.Database;
                return _connection
                    .Query("SHOW TABLES;")
                    .Select(t => 
                        {
                            string tableName = t[$"Tables_in_{databaseName}"];
                            var table = new Table {
                                Name = tableName,
                                Fields = getFields(tableName),
                            };
                            // table.Indices = getIndices(tableName)
                            return table;
                        })
                    .ToList();
            }
        }

        // private IEnumerable<string> getIndices(string tableName)
        // {
        //      return _connection
        //         .Query($"SELECT DISTINCT TABLE_NAME, INDEX_NAME FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '{tableName}';", 
        //         new {
        //             tableName
        //         })
        //         .Select(t => new Index {
        //             Name = t.INDEX_NAME
        //         })
        //         .ToList();
        // }

        private IEnumerable<Field> getFields(string tableName) // TODO: are we succeptible to injection attacks here
        {
            return _connection
                .Query($"SHOW COLUMNS FROM {tableName}", new {
                    tableName
                })
                .Select(t => new Field {
                    Name = t.Field,
                    Type = findType(t.Type)
                })
                .ToList();
        }

        private string findType(string type)
        {
            var matchedMapping = Program.MySqlMappings.FirstOrDefault(m => string.Equals(m.Item1, type, StringComparison.OrdinalIgnoreCase));

            if ( matchedMapping == null ) 
            {
                throw new Exception();
            }
            return matchedMapping.Item2;
        }

        private static string getDbType(string str)
        {
            var matchedMapping = Program.MySqlMappings.FirstOrDefault(m => string.Equals(m.Item2, str, StringComparison.OrdinalIgnoreCase));

            if ( matchedMapping == null ) 
            {
                throw new Exception();
            }
            return matchedMapping.Item1;
        }
        public void Apply(DbDiff diff)
        {
            string script = this.GenerateScript(diff);
            
            if ( string.IsNullOrEmpty(script)) return; // NOOP

            _connection.Execute(script);
        }

        public string GenerateScript(DbDiff diff)
        {
            StringBuilder builder = new StringBuilder();

            foreach (Table table in diff.AddedTables) 
            {

                builder.AppendLine($"CREATE TABLE {table.Name} (");

                foreach ( var field in table.Fields.Where(f => !f.Ignored))
                {
                    builder.AppendLine($"    {field.Name} {getDbType(field.Type)},");
                }

                foreach ( var index in table.Indices ) 
                {
                    var uniqueText = index.IsUnique ? "UNIQUE " : "";
                    builder.AppendLine($"    {uniqueText} KEY {index.Name} ({index.Fields.Aggregate((a, b) => a + ", " + b)}),");
                }

                if ( table.PrimaryKey != null ) 
                {
                    builder.AppendLine($"    PRIMARY KEY ({table.PrimaryKey.Aggregate((a, b) => a + ", " + b)}),");
                }

                builder.Remove(builder.Length - 2, 1);
                builder.AppendLine($");");
                builder.AppendLine();
            }

            foreach (TableModification mod in diff.ModifiedTables )
            {
                foreach (Field added in mod.AddedColumns){                    
                    builder.AppendLine($"ALTER TABLE {mod.Name} ADD COLUMN {added.Name} {getDbType(added.Type)};");
                }
                
                foreach (ColumnModification colMod in mod.ChangedColumns){
                    builder.AppendLine($"ALTER TABLE {mod.Name} MODIFY COLUMN {colMod.B.Name} {getDbType(colMod.A.Type)};");
                }

                foreach (Field removed in mod.RemovedColumns){
                    builder.AppendLine($"ALTER TABLE {mod.Name} DROP COLUMN {removed.Name};");
                }

                if ( mod.IsPrimaryKeyAdded ) 
                {
                    builder.AppendLine($"ALTER TABLE {mod.Name} ADD PRIMARY KEY ({mod.Out.PrimaryKey.Aggregate((a, b) => a + ", " + b)});");
                }

                if ( mod.IsPrimaryKeyChanged ) 
                {
                    builder.AppendLine($"ALTER TABLE {mod.Name} DROP PRIMARY KEY, ADD PRIMARY KEY ({mod.Out.PrimaryKey.Aggregate((a, b) => a + ", " + b)});");
                }

                if ( mod.IsPrimaryKeyRemoved ) 
                {
                    builder.AppendLine($"ALTER TABLE {mod.Name} DROP PRIMARY KEY;");
                }
            }

            foreach (Table table in diff.RemovedTables) {
                builder.AppendLine($"DROP TABLE {table.Name};");
                builder.AppendLine();
            }

            return builder.ToString();
        }
    } 
}