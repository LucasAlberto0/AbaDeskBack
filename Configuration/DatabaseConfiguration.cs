using Npgsql;
using System;

namespace AbaDeskBack.Configuration;

public static class DatabaseConfiguration
{
    public static string FormatConnectionString(string? rawConnectionString)
    {
        if (string.IsNullOrWhiteSpace(rawConnectionString))
            return string.Empty;

        if (!rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return rawConnectionString;
        }

        var uri = new Uri(rawConnectionString);
        var userInfo = uri.UserInfo.Split(':');
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }
}
