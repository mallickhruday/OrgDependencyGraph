# Organization Dependency Graph

This little utility queries your file system and finds out all the NuGet dependencies you have. This works based on the idea that you would clone all the repositories from your organization and dump them into one folder. Then, you would use that folder path and pass this onto this command-line utility.

The output of this will be dumped into Neo4j so that you can query this as you wish.

## Preparation

Clone all the repos under a folder. Do this by running the following bash script:

```bash
curl "https://api.github.com/orgs/{ORG-NAME}/repos?page=1&per_page=100&type=private&access_token={ACCESS-TOKEN-HERE}" | 
    grep -e 'ssh_url*' | 
    cut -d \" -f 4 | 
    xargs -L1 git clone $1 --depth 1
```

Increment the page count for enough times to consume all the repos.

## Usage

Run the below comment by supplying the correct arguments:

```bash
dotnet run "/Users/tugberk/apps/my-org" "http://localhost:7474/db/data" "neo4j" "neo4j"
```

> You can start an instance of a Neo4j database through Docker by running the following command:
>
> ```bash
> docker run \
>    --publish=7474:7474 --publish=7687:7687 \
>    --volume=$HOME/neo4j/data:/data \
>    --volume=$HOME/neo4j/logs:/logs \
>    neo4j:3.0
>```

## Querying the Data

The data should now be queryable through [Cypher Query Language](https://neo4j.com/developer/cypher-query-language/). Here are a few example of the things that you can find out:

### Count of Package Dependencies

```cypher
MATCH (pack:Package)<-[:DEPENDS_ON]-(proj:Project)
RETURN pack.id, count(proj)
ORDER BY count(proj) DESC
```

### A Particular Dependency and Its Projects

```cypher
MATCH (p:Project)-[:DEPENDS_ON]->(pack:Package)
WHERE pack.id = "Newtonsoft.Json"
RETURN p, pack
```

## Limitations

 - .NET Core SDK project system is not supported. This omly works for `packages.config` files.