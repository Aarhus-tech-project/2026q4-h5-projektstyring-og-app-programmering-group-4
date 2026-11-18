using Elwise.Api.Data;
using Elwise.Api.Prices;
using Elwise.Api.Security;
using Elwise.Core.Prices;
using Elwise.Infrastructure.EnergiDataService;
using Microsoft.EntityFrameworkCore;

if (args.Length == 2 && args[0] == "hash")
{
    Console.WriteLine(AdminPassword.Hash(args[1]));
    return;
}
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddDbContext<ElwiseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Elwise")));

builder.Services.AddHttpClient<EnergiDataServiceClient>();
builder.Services.AddScoped<PriceStore>(services => new PriceStore(
    services.GetRequiredService<ElwiseDbContext>(),
    services.GetRequiredService<EnergiDataServiceClient>()));

var app = builder.Build();

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

app.MapDelete("/api/admin/prices", async (
    HttpRequest request,
    IConfiguration configuration,
    PriceStore store,
    CancellationToken cancellationToken) =>
{
    string? password = request.Headers["X-Admin-Password"];
    string? savedHash = configuration["Admin:PasswordHash"];

    if (password is null || savedHash is null || !AdminPassword.Verify(password, savedHash))
    {
        return Results.Unauthorized();
    }

    int deleted = await store.DeleteAllAsync(cancellationToken);

    return Results.Ok(new { deleted });
});
app.Run();