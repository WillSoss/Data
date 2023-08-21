# DbDeploy

## Command-Line Interface

To create a DbDeploy CLI for your database:

1. Create a new console app
2. Add a package reference to `WillSoss.DbDeploy`.
3. In `async Task Main(string[] args)` in Program.cs, put the following code:
```c#
await DatabaseCli
    .CreateDefaultBuilder(args)
    .ConfigureDatabase(
        SqlDatabase
            .CreateBuilder()
            .WithConnectionString("default-connection-string-for-local-dev")
            .AddMigrations("c:\db\migrations"))
    .Build()
    .RunAsync(CancellationToken.None);
```
4. Build and run.

### CLI Reference

In this reference, the CLI console app is `dbdeploy.exe`.

#### Deploy a Database

The deploy CLI creates a database, if it does not exist, and runs migrations scripts in version order. Previously run migrations will not be rerun. If the CLI detects that a migration has been skipped it will not run any scripts to prevent the database from entering an invalid state.

```bash
# Create mydb on localhost, if not exists, and run migrations scripts against mydb
dbdeploy.exe --connectionstring "server=localhost;database=mydb;integrated security=true;trustservercertificate=true;"

# If a default connection string is set (WithConnectionString(...)), create and run migrations against the db
dbdeploy.exe

#Create and run migrations up to version 1.0.1
dbdeploy.exe migrate --version 1.0.1

# Drop the database if it exists first, then create and run migrations
dbdeploy.exe --drop
```

#### Create

The create command creates the database if it does not exist. If the database does exist, the database is not modified and the CLI will exit without error.

```bash
dbdeploy.exe create --connectionstring "server=localhost;database=mydb;integrated security=true;trustservercertificate=true;"

# Use the default connection string
dbdeploy.exe create
```

#### Drop

The drop command drops the database if it exists. If the database does not exist the CLI will exit without error.

```bash
dbdeploy.exe drop --connectionstring "server=localhost;database=mydb;integrated security=true;trustservercertificate=true;"

# Use the default connection string
dbdeploy.exe drop

# Disable production keyword detection to prevent destructive operations against production databases
dbdeploy.exe drop --unsafe --connectionstring "server=localhost;database=mydb_prod;integrated security=true;trustservercertificate=true;"
```

#### Migrate

The migrate command runs migration scripts in version order against the database. Previously run migrations will not be rerun. If the CLI detects that a migration has been skipped it will not run any scripts to prevent the database from entering an invalid state.

```bash
# Migrates mydb to the latest version
dbdeploy.exe migrate --connectionstring "server=localhost;database=mydb;integrated security=true;trustservercertificate=true;"

# Use the default connection string
dbdeploy.exe migrate

# Migrate up to version 1.0.1
dbdeploy.exe migrate --version 1.0.1
```

#### Run Scripts and Actions

The run command executes the script or action in the specified argument. Scripts and actions must be added to the database builder in the CLI configuration. Script and action names must be unique.

```c#
await DatabaseCli
    .CreateDefaultBuilder(args)
    .ConfigureDatabase(
        SqlDatabase
            .CreateBuilder()
            .WithConnectionString("default-connection-string-for-local-dev")
            .AddMigrations("c:\db\migrations")
            .AddNamedScript("c:\db\scripts\populate-test-data.sql")
            .AddAction("clear-table", db => ClearTable(db)))
    .Build()
    .RunAsync(CancellationToken.None);
```

```bash
dbdeploy.exe run populate-test-data
dbdeploy.exe run clear-tables
```