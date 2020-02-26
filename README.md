# dotnet.simple.redis
Simple Redis Client

### Testing
Testing this library boils down to running commands against an actual
`redis` instance and seeing if everything works. Fairly straightforward
concept.

From a continous build perspective, this is setup using `docker-compose`
which spins up a `redis` instance and executes the tests.

That clearly doesn't work for local development of the library so, 
`appsettings.json` is used to provide the connection string.
To ensure everything works, the connection string within this file
is the name of the `link` within `docker-compose`.

In order to test locally or against another `redis` instance simply add
an additional settings file named `appsettings.local.json` (not tracked) and 
add the connection string there. 

For now, only the `ConnectionString` is being used. This will need to change
in future iterations to test the other configuration values 
(e.g. `Keyspace`, `Port`, `ConnectionPool`)