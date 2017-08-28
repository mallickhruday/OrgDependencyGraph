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
            foreach (var fileName in Directory.GetFiles(basePath, "packages.config", SearchOption.AllDirectories))
            {
                var packagesFile = new PackageReferenceFile(fileName);
                foreach (var reference in packagesFile.GetPackageReferences())
                {
                    if(reference.TargetFramework != null) 
                    {
                        System.Console.WriteLine($"{reference.Id}: {reference.Version.ToFullString()} ({reference.TargetFramework.Identifier} [{reference.TargetFramework.FullName}])");
                    }
                    else 
                    {
                        System.Console.WriteLine($"{reference.Id}: {reference.Version.ToFullString()}");
                    }
                }
            }
        }
    }
}
