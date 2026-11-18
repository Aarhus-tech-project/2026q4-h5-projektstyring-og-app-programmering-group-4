using Elwise.Api.Data;
using Elwise.Api.Prices;
using Elwise.Core.Prices;
using Elwise.Infrastructure.EnergiDataService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


// The database is one SQLite file. The file name is in appsettings.json.
builder.Services.AddDbContext<ElwiseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Elwise")));

// The server gets missing prices from Energi Data Service with the same client as the app.
builder.Services.AddHttpClient<EnergiDataServiceClient>();
builder.Services.AddScoped<PriceStore>(services => new PriceStore(
    services.GetRequiredService<ElwiseDbContext>(),
    services.GetRequiredService<EnergiDataServiceClient>()));


var app = builder.Build();

// Creates the database file and the Prices table the first time the server starts.
using (IServiceScope scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ElwiseDbContext>().Database.EnsureCreated();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Example: GET /api/prices?area=DK1&firstDay=2026-11-18&dayCount=2
app.MapGet("/api/prices", async (
    PriceArea area,
    DateOnly firstDay,
    int dayCount,
    PriceStore store,
    CancellationToken cancellationToken) =>
{
    if (dayCount is < 1 or > 2)
    {
        return Results.BadRequest("dayCount must be 1 or 2.");
    }

    IReadOnlyList<PricePoint> prices =
        await store.GetPricesAsync(area, firstDay, dayCount, cancellationToken);


    return Results.Ok(prices);
});

app.Run();