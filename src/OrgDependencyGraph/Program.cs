using System;
using System.IO;
using NuGet;

namespace OrgDependencyGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Validate the path
            var basePath = args[0];
            foreach (var filePath in Directory.GetFiles(basePath, "packages.config", SearchOption.AllDirectories))
            {
                var projectName = Path.GetFileName(Path.GetDirectoryName(filePath));
                var repositoryName = GetRepositoryName(Path.GetDirectoryName(filePath));

                WriteSeperator();
                System.Console.WriteLine($"{projectName} ({repositoryName})");
                WriteSeperator();

                var packagesFile = new PackageReferenceFile(filePath);
                foreach (var reference in packagesFile.GetPackageReferences())
                {
                    if (reference.TargetFramework != null)
                    {
                        System.Console.WriteLine($"{reference.Id}: {reference.Version.ToFullString()} ({reference.TargetFramework.Identifier} [{reference.TargetFramework.FullName}])");
                    }
                    else
                    {
                        System.Console.WriteLine($"{reference.Id}: {reference.Version.ToFullString()}");
                    }
                }

                System.Console.WriteLine(Environment.NewLine);
            }
        } 

        private static string GetRepositoryName(string path)
        {
            if(Directory.Exists(Path.Combine(path, ".git"))) 
            {
                return Path.GetFileName(path);
            }

            return GetRepositoryName(Path.GetDirectoryName(path));
        }

        private static void WriteSeperator()
        {
            System.Console.WriteLine("============================================================================");
        }
    }
}
