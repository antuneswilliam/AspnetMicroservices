using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            int retryForAvailability = retry.Value;

            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;

            var configuration = services.GetRequiredService<IConfiguration>();

            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating postgresql database.");

                using var connection = new NpgsqlConnection
                    (configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

                connection.Open();

                using var command = new NpgsqlCommand
                {
                    Connection = connection
                };

                command.CommandText = @"CREATE TABLE IF NOT EXISTS Coupon (Id SERIAL PRIMARY KEY,
                    ProdutName VARCHAR(24) NOT NULL,
                    Description TEXT,
                    Amount INT)";

                command.ExecuteNonQuery();

                command.CommandText = @"INSERT INTO Coupon (ProductName, Description, Amount)
                    SELECT 'IPhone X' AS ProductName, 'IPhone Discount' AS Description, 150 AS Price
                    WHERE 0 = (SELECT count(*) FROM coupon)
                    UNION 
                    SELECT 'Samsung 10' AS ProductName, 'Samsung Discount' AS Description, 100 AS Price
                    WHERE 0 = (SELECT count(*) FROM coupon);";

                command.ExecuteNonQuery();

                logger.LogInformation("Migrated postresql database.");

            }
            catch (NpgsqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the postresql database");

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    System.Threading.Thread.Sleep(2000);
                    MigrateDatabase<TContext>(host, retryForAvailability);
                }
            }

            return host;
        }
    }
}
