using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections;
using WillSoss.DbDeploy.Sql;

namespace WillSoss.DbDeploy.Tests
{
    [Collection(nameof(DatabaseCollection))]
    [Trait("Category", "Migrations")]
    public class MigrateTests : DatabaseTest
    {

        public MigrateTests(DatabaseFixture fixture)
            : base(fixture, "test") { }
        
        [Fact]
        public void ShouldLoadMigrations()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            // Act
            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            // Assert
            db.Migrations.Count().Should().Be(10);
        }

        [Fact]
        public async Task ShouldApplyMigrations()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            await db.Create();

            // Act
            var count = await db.MigrateToLatest();

            // Assert
            count.Should().Be(10);

            var migrations = await db.GetAppliedMigrations(db.GetConnection());

            // 4 scripts + database create
            migrations.Count().Should().Be(11);
        }

        [Fact]
        public async Task ShouldApplyMigrationToVersion()
        {
            // Arrange
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(migrationsPath)
                .Build();

            await db.Create();

            await db.MigrateTo(new Version(0, 1));

            // Act
            var count = await db.MigrateToLatest();

            // Assert
            count.Should().Be(6);

            var migrations = await db.GetAppliedMigrations(db.GetConnection());

            // 10 scripts + database create
            migrations.Count().Should().Be(11);
        }

        [Fact]
        public async Task ShouldNotApplySkippedMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(new[] { MultipleVersionsMixedPreAndPost.Last() })
                .Build();

            await db.Create();

            await db.MigrateToLatest();

            db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(MultipleVersionsMixedPreAndPost)
                .Build();

            // Act
            var ex = await db.Invoking(db => db.MigrateToLatest())
                .Should().ThrowAsync<MissingMigrationsException>();

            // Assert
            ex.Subject.First().MissingScripts.Should()
                .BeEquivalentTo(MultipleVersionsMixedPreAndPost.Take(MultipleVersionsMixedPreAndPost.Count() - 1));
        }

        [Fact]
        public async Task ShouldApplyMissingMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(new[] { MultipleVersionsMixedPreAndPost.Last() })
                .Build();

            await db.Create();

            await db.MigrateToLatest();

            db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(MultipleVersionsMixedPreAndPost)
                .Build();


            // Act
            var count = await db.MigrateToLatest(true);

            // Assert
            count.Should().Be(3);

            var migrations = await db.GetAppliedMigrations(db.GetConnection());

            // 4 scripts + database create
            migrations.Count().Should().Be(5);
        }

        [Fact]
        public async Task WithMultipleVersionsPreThenPost_ShouldApplyMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(MultipleVersionsPreThenPost)
                .Build();

            await db.Create();

            // Act
            var count = await db.MigrateToLatest();

            // Assert
            count.Should().Be(4);
        }

        [Fact]
        public async Task WithPreFlagAndPreThenPost_ShouldApplyPreMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(PreThenPost)
                .Build();

            await db.Create();

            // Act
            var count = await db.MigrateTo(null, MigrationPhase.Pre);

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public async Task WithPreFlagAndOnlyPost_ShouldApplyZeroMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(OnlyPost)
                .Build();

            await db.Create();

            // Act
            var count = await db.MigrateTo(null, MigrationPhase.Pre);

            // Assert
            count.Should().Be(0);
        }

        [Fact]
        public async Task WithPreFlagAndPreAfterPost_ShouldNotApplyMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(PrePostPre)
                .Build();

            await db.Create();

            // ACT
            var ex = await db.Invoking(db => db.MigrateTo(null, MigrationPhase.Pre))
                .Should().ThrowAsync<UnableToMigrateException>();

            // ASSERT
        }

        [Fact]
        public async Task WithPreFlagAndPostThenPre_ShouldNotApplyMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(PostThenPre)
                .Build();

            await db.Create();

            // ACT
            var ex = await db.Invoking(db => db.MigrateTo(null, MigrationPhase.Pre))
                .Should().ThrowAsync<UnableToMigrateException>();

            // ASSERT
        }

        [Fact]
        public async Task WithPostFlagAndOnlyPost_ShouldApplyMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(OnlyPost)
                .Build();

            await db.Create();

            // Act
            var count = await db.MigrateTo(null, MigrationPhase.Post);

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public async Task WithPostFlagAndOnlyPre_ShouldNotApplyMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(OnlyPre)
                .Build();

            await db.Create();

            // ACT
            var ex = await db.Invoking(db => db.MigrateTo(null, MigrationPhase.Post))
                .Should().ThrowAsync<UnableToMigrateException>();

            // ASSERT
        }

        [Fact]
        public async Task WithPostFlagAndPostThenPre_ShouldNotApplyMigrations()
        {
            // Arrange

            var db = SqlDatabase.CreateBuilder()
                .WithConnectionString(ConnectionString)
                .AddMigrations(PostThenPre)
                .Build();

            await db.Create();

            // ACT
            var ex = await db.Invoking(db => db.MigrateTo(null, MigrationPhase.Post))
                .Should().ThrowAsync<UnableToMigrateException>();

            // ASSERT
        }

        private static IEnumerable<MigrationScript> MultipleVersionsPreThenPost => new[]
        {
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("2.0"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("3.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("4.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql"))
        };

        private static IEnumerable<MigrationScript> MultipleVersionsMixedPreAndPost => new[]
        {
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("2.0"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("2.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql"))
        };

        private static IEnumerable<MigrationScript> OnlyPre => new[]
{
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("1.1"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
        };

        private static IEnumerable<MigrationScript> OnlyPost => new[]
        {
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("1.1"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
        };

        private static IEnumerable<MigrationScript> PreThenPost => new[]
        {
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
        };

        private static IEnumerable<MigrationScript> PostThenPre => new[]
{
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("1.1"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
        };

        private static IEnumerable<MigrationScript> PrePostPre => new[]
{
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("1.0"), MigrationPhase.Post, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql")),
            new MigrationScript(Version.Parse("1.1"), MigrationPhase.Pre, Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "0.1", "_pre", "01 one.sql"))
        };
    }
}
