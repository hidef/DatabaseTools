using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Runtime.Loader;
using DatabaseTools.Model;
using DatabaseTools.Sources.Code;
using DatabaseTools.Sources.MySQL;
using DatabaseTools.Sources.PostgreSQL;

namespace DatabaseTools
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Discovering Database Changes.");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    { "from", discoverDatabase() }
                })
                ;

            var configuration = builder.Build();

            IDb input = GetSource(configuration["from"]);
            IDb output = GetSource(configuration["to"]);
            
            DbDiff diff = new DiffGenerator().Diff(input.GetModel(),output.GetModel());

            Console.WriteLine(JsonConvert.SerializeObject(diff, Formatting.Indented));
 
            string diffScript = output.GenerateScript(diff);

            Console.WriteLine(diffScript);
            
            if ( bool.Parse(configuration["apply"] ?? bool.FalseString ) )
            {
                output.Apply(diff);
            }
        }

        private static string discoverDatabase()
        {
            var projectName = new DirectoryInfo(".");
            string path = Path.Combine(projectName.FullName, "bin", "Debug", "netcoreapp1.1", projectName.Name + ".dll");
            
            var assemblyName = new FileInfo(path);

            if ( !assemblyName.Exists ) {
                Console.WriteLine($"Unable to load assembly: {assemblyName.FullName} - {path}");
                throw new Exception();
            }

            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName.FullName);

            return "type://" + myAssembly.GetType($"{myAssembly.GetName().Name}.Database").AssemblyQualifiedName;
        }

        private static IDb GetSource(string connectionString)
        {
            if ( string.IsNullOrEmpty(connectionString) ) throw new ArgumentNullException(nameof(connectionString));

            Console.WriteLine($"Loading: {connectionString}");
            
            Uri connectionUri = new Uri(connectionString);

            return registrations[connectionUri.Scheme](connectionString);
        }

        static Dictionary<string, Func<string, IDb>> registrations = new Dictionary<string, Func<string, IDb>> {
            {"mysql", uri => new MySqlDb(new Uri(uri))},
            {"postgresql", uri => new PostgreSqlDb(new Uri(uri))},
            {"file", uri => new RoslynDB(uri)},
            {"type", uri => new CSharpDbDefiniton(Type.GetType(new Uri(uri).LocalPath))}
        };
    }
}
