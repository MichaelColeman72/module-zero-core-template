using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace AbpCompanyName.AbpProjectName.EntityFrameworkCore
{
    public static class AbpProjectNameDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AbpProjectNameDbContext> builder, string connectionString)
        {
            _ = builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<AbpProjectNameDbContext> builder, DbConnection connection)
        {
            _ = builder.UseSqlServer(connection);
        }
    }
}
