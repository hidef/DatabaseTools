using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Loader;

namespace DatabaseTools
{
    public class Program
    {

        public static IEnumerable<Tuple<string, string>> MySqlMappings = new [] {
            Tuple.Create("BOOL NOT NULL","bool"),
            Tuple.Create("BOOL NULL","bool?	"),
            Tuple.Create("BOOL","bool"),
            Tuple.Create("TINYINT NOT NULL","sbyte"),
            Tuple.Create("TINYINT NULL","sbyte?"),
            Tuple.Create("TINYINT","sbyte"),
            Tuple.Create("TINYINT UNSIGNED NOT NULL","byte"),
            Tuple.Create("TINYINT UNSIGNED NULL","byte?"),
            Tuple.Create("TINYINT UNSIGNED","byte"),
            Tuple.Create("TINYINT(1)","byte"),
            Tuple.Create("SMALLINT NOT NULL","short"),
            Tuple.Create("SMALLINT NULL","short?"),
            Tuple.Create("SMALLINT","short"),
            Tuple.Create("SMALLINT UNSIGNED NOT NULL","ushort"),
            Tuple.Create("SMALLINT UNSIGNED NULL","ushort?"),
            Tuple.Create("SMALLINT UNSIGNED","ushort"),
            Tuple.Create("INT NOT NULL","int"),
            Tuple.Create("INT NULL","int?"),
            Tuple.Create("INT","int"),
            Tuple.Create("INT UNSIGNED NOT NULL","uint"),
            Tuple.Create("INT UNSIGNED NULL","uint?"),
            Tuple.Create("INT UNSIGNED","uint"),
            Tuple.Create("BIGINT NOT NULL","long"),
            Tuple.Create("BIGINT NULL","long?"),
            Tuple.Create("BIGINT","long"),
            Tuple.Create("BIGINT UNSIGNED NOT NULL","ulong"),
            Tuple.Create("BIGINT UNSIGNED NULL","ulong?"),
            Tuple.Create("BIGINT UNSIGNED","ulong"),
            Tuple.Create("FLOAT NOT NULL","float"),
            Tuple.Create("FLOAT NULL","float?"),
            Tuple.Create("FLOAT","float"),
            Tuple.Create("DOUBLE NOT NULL","double"),
            Tuple.Create("DOUBLE NULL","double?"),
            Tuple.Create("DOUBLE","double"),
            Tuple.Create("DECIMAL NOT NULL","decimal"),
            Tuple.Create("DECIMAL NULL","decimal?"),
            Tuple.Create("DECIMAL","decimal"),
            Tuple.Create("CHARACTER NOT NULL","	char"),
            Tuple.Create("CHARACTER NULL","char?"),
            Tuple.Create("CHARACTER","char"),
            Tuple.Create("VARCHAR(50) NOT NULL","string"),
            Tuple.Create("VARCHAR(50) NULL","string"),
            Tuple.Create("VARCHAR(50)","string"),
            Tuple.Create("MEDIUMTEXT","string"),
            Tuple.Create("DATETIME NOT NULL","datetime"),
            Tuple.Create("DATETIME NULL","datetime?"),
            Tuple.Create("DATETIME","datetime"),
            Tuple.Create("BLOB","byte[]"),
            Tuple.Create("TEXT", "string")
        };

        public static IEnumerable<Tuple<string, string>> CSharpMappings = new [] {
            Tuple.Create("Int64","int64"),
            Tuple.Create("Long","long"),
            Tuple.Create("Boolean","boolean"),
            Tuple.Create("Byte[]","byte[]"),
            Tuple.Create("DateTime","datetime"),
            Tuple.Create("Double","double"),
            Tuple.Create("Int32","int32"),
            Tuple.Create("Int","int"),
            Tuple.Create("Decimal","decimal"),
            Tuple.Create("Decimal","decimal"),
            Tuple.Create("Single","single"),
            Tuple.Create("Int16","int16"),
            Tuple.Create("Short","short"),
            Tuple.Create("String","string"),
            Tuple.Create("DateTime","datetime"),
            Tuple.Create("DateTime","datetime"),
            Tuple.Create("DateTime","datetime"),
            Tuple.Create("DateTime","datetime"),
            Tuple.Create("TimeSpan","timespan"),
            Tuple.Create("String","string"),
            Tuple.Create("IPAddress","ipaddress"),
            Tuple.Create("Boolean","boolean"),
            Tuple.Create("Guid","guid"),
            Tuple.Create("Array","array")
        };

        public static IEnumerable<Tuple<string, string>> PostGresMappings = new [] {
            Tuple.Create("int8","Int64"),
            Tuple.Create("int8","Long"),
            Tuple.Create("bool","Boolean"),
            Tuple.Create("bytea","Byte[]"),
            Tuple.Create("date","DateTime"),
            Tuple.Create("float8","Double"),
            Tuple.Create("int4","Int32"),
            Tuple.Create("int4","Int"),
            Tuple.Create("money","Decimal"),
            Tuple.Create("numeric","Decimal"),
            Tuple.Create("float4","Single"),
            Tuple.Create("int2","Int16"),
            Tuple.Create("int2","Short"),
            Tuple.Create("text","String"),
            Tuple.Create("time","DateTime"),
            Tuple.Create("timetz","DateTime"),
            Tuple.Create("timestamp","DateTime"),
            Tuple.Create("timestamptz","DateTime"), 
            Tuple.Create("interval","TimeSpan"),
            Tuple.Create("varchar","String"),
            Tuple.Create("inet","IPAddress"),
            Tuple.Create("bit","Boolean"),
            Tuple.Create("uuid","Guid"),
            Tuple.Create("array","Array")
        };

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddInMemoryCollection(new Dictionary<string, string> {
                    { "from", discoverDatabase() }
                })
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();
            
            IDb source = GetSource(configuration["from"]);
            IDb destination = GetSource(configuration["to"]);
            
            DbDiff diff = Diff(source, destination);

            Console.WriteLine(JsonConvert.SerializeObject(diff, Formatting.Indented));
 
            string diffScript = destination.GenerateScript(diff);

            Console.WriteLine(diffScript);
            
            destination.Apply(diff);

        }

        private static string discoverDatabase()
        {
            string projectName = new DirectoryInfo(".").Name;
            string path = Path.Combine("bin", "Debug", "netcoreapp1.0", projectName + ".dll");
            
            var assemblyName = new FileInfo(path);

            if ( !assemblyName.Exists ) {
                Console.WriteLine($"Unable to load assembly: {assemblyName.FullName} - {path}");
                throw new Exception();
            }

            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName.FullName);

            return myAssembly.GetType($"{myAssembly.FullName.Split(',')[0]}.Database").AssemblyQualifiedName;
        }

        private static DbDiff Diff(IDb source, IDb destination)
        {
            return new DbDiff 
            {
                AddedTables = getAddedTables(source, destination),
                ModifiedTables = getModifiedTables(source, destination),
                RemovedTables = getRemovedTables(source, destination),
            };          
        }

        private static IEnumerable<Table> getAddedTables(IDb source, IDb destination)
        {
            var _in = source
                .TableTypes
                .ToList();
            var _out = destination.TableTypes.ToList();

            return _in
                .Where(t => !_out.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static IEnumerable<TableModification> getModifiedTables(IDb source, IDb destination)
        {
            var _in = source
                .TableTypes
                .ToList();
            var _out = destination.TableTypes.ToList();

            var coexistingTables = _in.Select(t => new {
                In = t,
                Out = _out.SingleOrDefault(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase))
            })
            .Where(p => p.Out != null);

            return coexistingTables
                .Select(t => new TableModification(t.In, t.Out) { Name = t.In.Name})
                .Where(tm => tm.Added.Count() + tm.Changes.Count() + tm.Removed.Count() > 0)
                .ToList();
        }

        private static IEnumerable<Table> getRemovedTables(IDb source, IDb destination)
        {
            var _in = source
                .TableTypes
                .ToList();
            var _out = destination.TableTypes.ToList();

            return _out
                .Where(t => !_in.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static IDb GetSource(string connectionString)
        {
            Console.WriteLine($"Loading: {connectionString}");
            
            var type = Type.GetType(connectionString);
            
            if ( type != null ) return new CSharpDbDefiniton(type);

            return new SqlDb(new Uri(connectionString));
        }
    }
}
