# Organization Dependency Graph

This little utility queries your file system and finds out all the NuGet dependencies you have. This works based on the idea that you would clone all the repositories from your organization and dump them into one folder. Then, you would use that folder path and pass this onto this command-line utility.

The output of this will be dumped into Neo4j so that you can query this as you wish.

## Preparation

## Usage

> TODO: This is incomplete, it will also require Neo4j connection details

```bash
dotnet run "/Users/tugberk/apps/my-org" "http://localhost:7474/db/data" "neo4j" "neo4j"
```

## Limitations

 - .NET Core SDK project system is not supported. This omly works for `packages.config` files.