using System;
using System.IO;

namespace OrgDependencyGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Validate the path
            var basePath = args[0];
            foreach (var item in Directory.GetFiles(basePath, "packages.config", SearchOption.AllDirectories))
            {
                System.Console.WriteLine(item);
            }
        }
    }
}
