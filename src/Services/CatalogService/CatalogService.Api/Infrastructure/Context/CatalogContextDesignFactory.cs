using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
                .UseSqlServer("Server=OnsellDb; initial Catalog=OnSellCatalogDb;User=sa; Password=1234567Da*;Trusted_Connection=false; Encrypt=True;TrustServerCertificate=True");

            return new CatalogContext(optionsBuilder.Options);
        }
    }
}
