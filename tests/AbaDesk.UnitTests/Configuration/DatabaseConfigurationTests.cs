using AbaDeskBack.Configuration;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AbaDesk.UnitTests.Configuration;

public class DatabaseConfigurationTests
{
    [Fact]
    public void DeveRetornarMesmaStringQuandoPadraoAdoNet()
    {
        var original = "Host=localhost;Port=5432;Database=abadesk_db;Username=postgres;Password=root";
        var result = DatabaseConfiguration.FormatConnectionString(original);
        result.Should().Be(original);
    }

    [Fact]
    public void DeveRetornarStringVaziaQuandoEntradaForNulaOuVazia()
    {
        DatabaseConfiguration.FormatConnectionString(null).Should().BeEmpty();
        DatabaseConfiguration.FormatConnectionString("").Should().BeEmpty();
        DatabaseConfiguration.FormatConnectionString("   ").Should().BeEmpty();
    }

    [Fact]
    public void DeveConverterUriPostgresqlParaFormatoAdoNetComSslRequire()
    {
        var uri = "postgresql://neondb_owner:npg_EGu3DSX6ekWy@ep-round-math-b6shy0no.c-2.sa-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require";
        var result = DatabaseConfiguration.FormatConnectionString(uri);

        var builder = new NpgsqlConnectionStringBuilder(result);
        builder.Host.Should().Be("ep-round-math-b6shy0no.c-2.sa-east-1.aws.neon.tech");
        builder.Username.Should().Be("neondb_owner");
        builder.Password.Should().Be("npg_EGu3DSX6ekWy");
        builder.Database.Should().Be("neondb");
        builder.Port.Should().Be(5432);
        builder.SslMode.Should().Be(SslMode.Require);
    }

    [Fact]
    public void DeveConverterUriPostgresqlPooledComSucesso()
    {
        var uri = "postgresql://neondb_owner:npg_EGu3DSX6ekWy@ep-round-math-b6shy0no-pooler.c-2.sa-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require";
        var result = DatabaseConfiguration.FormatConnectionString(uri);

        var builder = new NpgsqlConnectionStringBuilder(result);
        builder.Host.Should().Be("ep-round-math-b6shy0no-pooler.c-2.sa-east-1.aws.neon.tech");
        builder.Database.Should().Be("neondb");
        builder.SslMode.Should().Be(SslMode.Require);
    }
}
