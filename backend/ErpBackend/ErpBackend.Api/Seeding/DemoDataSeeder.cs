using ErpBackend.Api.Modules.Customer;
using ErpBackend.Api.Modules.Product;
using ErpBackend.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ErpBackend.Api.Seeding;

/// <summary>
/// DEMO-ONLY business data so the public demo looks real out of the box. Seeds Customers and Products
/// (the EF-backed generated modules) when <c>Demo:Seed=true</c> and the tables are empty. Idempotent;
/// runs after the identity seeders. Disabled by default — real installs leave <c>Demo:Seed</c> unset.
/// </summary>
public sealed class DemoDataSeeder(DbContext context, IConfiguration configuration) : IErpDataSeeder
{
    public int Order => 20;

    private static readonly string[] CompanyNames =
    [
        "Acme Corporation", "Globex", "Initech", "Umbrella Industries", "Soylent Corp", "Stark Industries",
        "Wayne Enterprises", "Wonka Industries", "Cyberdyne Systems", "Hooli", "Pied Piper", "Vandelay Industries",
        "Massive Dynamic", "Gekko & Co", "Bluth Company", "Dunder Mifflin", "Prestige Worldwide", "Sterling Cooper",
        "Oscorp", "Tyrell Corporation",
    ];

    private static readonly string[] ProductNames =
    [
        "Wireless Mouse", "Mechanical Keyboard", "27\" Monitor", "USB-C Hub", "Laptop Stand", "Webcam HD",
        "Noise-cancelling Headset", "Ergonomic Chair", "Standing Desk", "Docking Station", "External SSD 1TB",
        "Label Printer", "Barcode Scanner", "Conference Speaker", "Network Switch 8-port", "UPS 650VA",
        "Tablet 10\"", "Stylus Pen", "Cable Organizer", "Desk Lamp",
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("Demo:Seed"))
        {
            return;
        }
        await SeedCustomersAsync(cancellationToken);
        await SeedProductsAsync(cancellationToken);
    }

    private async Task SeedCustomersAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Customer>().IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        for (var i = 0; i < CompanyNames.Length; i++)
        {
            var name = CompanyNames[i];
            context.Set<Customer>().Add(new Customer
            {
                Code = $"C-{i + 1:D3}",
                Name = name,
                Email = $"contact@{Slug(name)}.example",
                Segment = i % 3 == 0 ? "enterprise" : "smb",
                IsActive = i % 7 != 0,
            });
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedProductsAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Product>().IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        for (var i = 0; i < ProductNames.Length; i++)
        {
            context.Set<Product>().Add(new Product
            {
                Sku = $"SKU-{i + 1:D3}",
                Name = ProductNames[i],
                Price = 9.99m + i * 5m,
                Stock = (i * 7) % 50,
                IsActive = i % 6 != 0,
            });
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private static string Slug(string value)
        => new string(value.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c)).ToArray());
}
