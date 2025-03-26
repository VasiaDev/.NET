using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DispensaryApp.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DispensaryDbContext>
    {
        public DispensaryDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            var optionsBuilder = new DbContextOptionsBuilder<DispensaryDbContext>();
            optionsBuilder.UseMySql(connectionString, 
                new MySqlServerVersion(new Version(8, 0, 0)),
                options => options.EnableRetryOnFailure());

            return new DispensaryDbContext(optionsBuilder.Options);
        }
    }
} 