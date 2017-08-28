using System;
using System.IO;
using System.Threading.Tasks;
using Neo4jClient;
using Newtonsoft.Json.Serialization;
using NuGet;

namespace OrgDependencyGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Validate inputs
            var basePath = args[0];
            var neo4jUri = args[1];
            var neo4jUsername = args[2];
            var neo4jPassword = args[3];

            using (var graphClient = new GraphClient(new Uri(neo4jUri), neo4jUsername, neo4jPassword))
            {
                graphClient.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
                graphClient.Connect();

                ConfigureAsync(graphClient).Wait();

                foreach (var filePath in Directory.GetFiles(basePath, "packages.config", SearchOption.AllDirectories))
                {
                    var projectName = Path.GetFileName(Path.GetDirectoryName(filePath));
                    var repositoryName = GetRepositoryName(Path.GetDirectoryName(filePath));

                    WriteSeperator();
                    System.Console.WriteLine($"{projectName} ({repositoryName})");
                    WriteSeperator();

                    var projectGraphItem = new ProjectGraphItem
                    {
                        Name = projectName,
                        RepositoryName = repositoryName
                    };

                    AddToNeo4j(graphClient, projectGraphItem);

                    var packagesFile = new PackageReferenceFile(filePath);
                    foreach (var reference in packagesFile.GetPackageReferences())
                    {
                        AddToNeo4j(graphClient, reference);

                        if (reference.TargetFramework != null)
                        {
                            System.Console.WriteLine($"{reference.Id}: {reference.Version.ToFullString()} ({reference.TargetFramework.Identifier} [{reference.TargetFramework.FullName}])");
                        }
                        else
                        {
                            System.Console.WriteLine($"{reference.Id}: {reference.Version.ToFullString()}");
                        }

                        graphClient.Cypher
                            .Match($"(package:{Labels.Package}),(project:{Labels.Project})")
                            .Where((ProjectGraphItem project) => project.Id == projectGraphItem.Id)
                            .AndWhere((PackageGraphItem package) => package.Id == reference.Id)
                            .Merge($"(project)-[:{Relationships.DependsOn}{{ version: {{ version }}, targetFramework: {{ targetFramework }} }}]->(package)")
                            .WithParams(new 
                            {
                                Version = reference.Version.ToFullString(),
                                TargetFramework = reference.TargetFramework?.Identifier ?? ""
                            })
                            .ExecuteWithoutResults();
                    }

                    System.Console.WriteLine(Environment.NewLine);
                }
            }
        }

        private static void AddToNeo4j(GraphClient graphClient, PackageReference reference)
        {
            var packageGraphItem = new PackageGraphItem
            {
                Id = reference.Id
            };

            graphClient.Cypher
                .Merge($"(package:{Labels.Package} {{ id: {{id}} }})")
                .OnCreate().Set("package = {package}")
                .WithParams(new
                {
                    Id = packageGraphItem.Id,
                    Package = packageGraphItem
                })
                .ExecuteWithoutResults();
        }

        private static void AddToNeo4j(GraphClient graphClient, ProjectGraphItem projectGraphItem)
        {
            graphClient.Cypher
                .Merge($"(project:{Labels.Project} {{ id: {{id}} }})")
                .OnCreate().Set("project = {project}")
                .WithParams(new
                {
                    Id = projectGraphItem.Id,
                    Project = projectGraphItem
                })
                .ExecuteWithoutResults();
        }

        private static string GetRepositoryName(string path)
        {
            if (Directory.Exists(Path.Combine(path, ".git")))
            {
                return Path.GetFileName(path);
            }

            return GetRepositoryName(Path.GetDirectoryName(path));
        }

        private static void WriteSeperator()
        {
            System.Console.WriteLine("============================================================================");
        }

        private static async Task ConfigureAsync(IGraphClient graphClient)
        {
            await graphClient.Cypher.Create($"CONSTRAINT ON (p:{Labels.Package}) ASSERT p.id IS UNIQUE")
                .ExecuteWithoutResultsAsync()
                .ConfigureAwait(false);

            await graphClient.Cypher.Create($"CONSTRAINT ON (p:{Labels.Project}) ASSERT p.id IS UNIQUE")
                .ExecuteWithoutResultsAsync()
                .ConfigureAwait(false);
        }
    }

    public static class Relationships
    {
        public const string DependsOn = "DEPENDS_ON";
    }

    public static class Labels
    {
        public const string Project = "Project";
        public const string Package = "Package";
    }

    public class ProjectGraphItem
    {
        public string Id => $"{RepositoryName}-{Name}";
        public string RepositoryName { get; set; }
        public string Name { get; set; }
    }

    public class PackageGraphItem
    {
        public string Id { get; set; }
    }
}
