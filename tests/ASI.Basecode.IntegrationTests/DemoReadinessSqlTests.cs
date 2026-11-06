using ASI.Basecode.WebApp.Readiness;
using Microsoft.Data.SqlClient;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public sealed class DemoReadinessSqlTests(SqlServerDatabaseFixture fixture) : IClassFixture<SqlServerDatabaseFixture>
{
    [Fact]
    public async Task RuntimeValidatorRejectsWrongDatabaseBeforeAuditProbe()
    {
        await using var connection = OpenTestConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var before = await CountProbesAsync(connection);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DemoReadiness.CheckSqlConnectionAsync(connection, "irrelevant"));

        Assert.Equal("Wrong runtime database.", error.Message);
        Assert.Equal(before, await CountProbesAsync(connection));
        Assert.Equal(0, await ScalarIntAsync(connection, "SELECT @@TRANCOUNT"));
    }

    [Fact]
    public async Task ProvisioningScriptRejectsDisposableDatabaseWithoutChangingPermissions()
    {
        await using var connection = OpenTestConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var user = await CreateRestrictedUserAsync(connection);
        var before = await ScalarIntAsync(connection, $"SELECT COUNT(*) FROM sys.database_permissions WHERE grantee_principal_id = DATABASE_PRINCIPAL_ID(N'{user}')");
        var path = Path.Combine(AppContext.BaseDirectory, "database", "provision-demo-runtime.sql");
        var script = (await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken)).Replace("$(RuntimeUser)", user, StringComparison.Ordinal);

        var error = await Assert.ThrowsAsync<SqlException>(() =>
            SqlServerDatabaseFixture.ExecuteScriptAsync(connection, script, TestContext.Current.CancellationToken));

        Assert.Equal(51100, error.Number);
        Assert.Equal(before, await ScalarIntAsync(connection, $"SELECT COUNT(*) FROM sys.database_permissions WHERE grantee_principal_id = DATABASE_PRINCIPAL_ID(N'{user}')"));
        Assert.Equal(0, await ScalarIntAsync(connection, "SELECT @@TRANCOUNT"));
    }

    [Theory]
    [InlineData("UPDATE")]
    [InlineData("DELETE")]
    public async Task EffectiveAuditMutationPermissionIsRejectedBeforeInsert(string permission)
    {
        await using var connection = OpenTestConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var user = await CreateRestrictedUserAsync(connection);
        await ExecuteAsync(connection, $"GRANT {permission} ON dbo.activity_logs TO [{user}]");
        await InsertBaselineAsync(connection);
        var before = await CountProbesAsync(connection);
        var identityBefore = await AuditIdentityAsync(connection);
        await ExecuteAsync(connection, $"EXECUTE AS USER = '{user}'");

        await Assert.ThrowsAsync<InvalidOperationException>(() => DemoReadiness.CheckAuditAsync(connection));

        Assert.Equal(0, await ScalarIntAsync(connection, "SELECT @@TRANCOUNT"));
        await ExecuteAsync(connection, "REVERT");
        Assert.Equal(before, await CountProbesAsync(connection));
        Assert.Equal(identityBefore, await AuditIdentityAsync(connection));
    }

    [Fact]
    public async Task InheritedAuditMutationPermissionIsRejected()
    {
        await using var connection = OpenTestConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var user = await CreateRestrictedUserAsync(connection);
        await ExecuteAsync(connection, $"ALTER ROLE db_datawriter ADD MEMBER [{user}]");
        await InsertBaselineAsync(connection);
        var identityBefore = await AuditIdentityAsync(connection);
        await ExecuteAsync(connection, $"EXECUTE AS USER = '{user}'");
        await Assert.ThrowsAsync<InvalidOperationException>(() => DemoReadiness.CheckAuditAsync(connection));
        Assert.Equal(0, await ScalarIntAsync(connection, "SELECT @@TRANCOUNT"));
        await ExecuteAsync(connection, "REVERT");
        Assert.Equal(identityBefore, await AuditIdentityAsync(connection));
    }

    [Fact]
    public async Task MissingAuditInsertPermissionFailsBeforeStartingTransaction()
    {
        await using var connection = OpenTestConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var user = await CreateRestrictedUserAsync(connection);
        await ExecuteAsync(connection, $"REVOKE INSERT ON dbo.activity_logs FROM [{user}]");
        await ExecuteAsync(connection, $"EXECUTE AS USER = '{user}'");
        await Assert.ThrowsAsync<InvalidOperationException>(() => DemoReadiness.CheckAuditAsync(connection));
        Assert.Equal(0, await ScalarIntAsync(connection, "SELECT @@TRANCOUNT"));
        await ExecuteAsync(connection, "REVERT");
    }

    [Fact]
    public async Task RestrictedAuditInsertSucceedsAndIsRolledBack()
    {
        await using var connection = OpenTestConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var user = await CreateRestrictedUserAsync(connection);
        await InsertBaselineAsync(connection);
        var before = await CountProbesAsync(connection);
        var identityBefore = await AuditIdentityAsync(connection);
        await ExecuteAsync(connection, $"EXECUTE AS USER = '{user}'");

        await DemoReadiness.CheckAuditAsync(connection);

        Assert.Equal(0, await ScalarIntAsync(connection, "SELECT @@TRANCOUNT"));
        await ExecuteAsync(connection, "REVERT");
        Assert.Equal(before, await CountProbesAsync(connection));
        Assert.Equal(identityBefore + 1, await AuditIdentityAsync(connection));
    }

    [Fact]
    public async Task FailedAuditInsertRollsBackAndLeavesConnectionUsable()
    {
        await using var connection = OpenTestConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var user = await CreateRestrictedUserAsync(connection);
        await InsertBaselineAsync(connection);
        var before = await CountProbesAsync(connection);
        var identityBefore = await AuditIdentityAsync(connection);
        var trigger = "readiness_failure_" + Guid.NewGuid().ToString("N");
        await ExecuteAsync(connection, $@"CREATE TRIGGER dbo.[{trigger}] ON dbo.activity_logs AFTER INSERT AS
            IF EXISTS (SELECT 1 FROM inserted WHERE action = 'readiness.probe')
                THROW 51991, 'Injected readiness failure', 1;");
        try
        {
            await ExecuteAsync(connection, $"EXECUTE AS USER = '{user}'");
            var error = await Assert.ThrowsAsync<SqlException>(() => DemoReadiness.CheckAuditAsync(connection));
            Assert.Equal(51991, error.Number);
            Assert.Equal(0, await ScalarIntAsync(connection, "SELECT @@TRANCOUNT"));
            await ExecuteAsync(connection, "REVERT");
            Assert.Equal(before, await CountProbesAsync(connection));
            Assert.Equal(identityBefore + 1, await AuditIdentityAsync(connection));
        }
        finally
        {
            // Cleanup on an independent administrator connection even if the assertion fails.
            await using var cleanup = OpenTestConnection();
            await cleanup.OpenAsync(TestContext.Current.CancellationToken);
            await ExecuteAsync(cleanup, $"DROP TRIGGER IF EXISTS dbo.[{trigger}]");
        }
    }

    private SqlConnection OpenTestConnection() => new(new SqlConnectionStringBuilder(fixture.ConnectionString)
    {
        Pooling = false
    }.ConnectionString);

    private static async Task<string> CreateRestrictedUserAsync(SqlConnection connection)
    {
        var user = "readiness_" + Guid.NewGuid().ToString("N");
        await ExecuteAsync(connection, $"CREATE USER [{user}] WITHOUT LOGIN; GRANT SELECT, INSERT ON dbo.activity_logs TO [{user}];");
        return user;
    }

    private static Task<int> CountProbesAsync(SqlConnection connection) =>
        ScalarIntAsync(connection, "SELECT COUNT(*) FROM dbo.activity_logs WHERE action = 'readiness.probe'");

    private static async Task<long> AuditIdentityAsync(SqlConnection connection)
    {
        await using var command = new SqlCommand("SELECT IDENT_CURRENT('dbo.activity_logs')", connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private static Task InsertBaselineAsync(SqlConnection connection) => ExecuteAsync(connection,
        "INSERT dbo.activity_logs (actor_type_id, action, entity_type, entity_id, description) VALUES (2, 'readiness.test-baseline', 'readiness', 0, N'test baseline')");

    private static async Task<int> ScalarIntAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
