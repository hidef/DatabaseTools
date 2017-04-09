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
               return _connection
                    .Query("SHOW TABLES;")
                    .Select(t => new Table {
                        Name = t.Tables_in_heroku_9a95a5b89d7b826,
                        Fields = getFields(t.Tables_in_heroku_9a95a5b89d7b826)
                    })
                    .ToList();
            }
        }

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

            foreach (Table table in diff.AddedTables) {
                builder.AppendLine($"CREATE TABLE {table.Name} (");
                foreach ( var field in table.Fields.Where(f => !f.Ignored))
                {
                    builder.AppendLine($"    {field.Name} {getDbType(field.Type)},");
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
            }

            foreach (Table table in diff.RemovedTables) {
                builder.AppendLine($"DROP TABLE {table.Name};");
                builder.AppendLine();
            }

            return builder.ToString();
        }
    } 
}