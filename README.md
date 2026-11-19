# Elwise

Elwise is a .NET 10 MAUI app that finds the cheapest time to start the dishwasher. It uses Danish spot prices for every 15 minutes from Energi Data Service and never suggests 17–21, when the grid is under the most pressure. The app gets its prices from its own server, Elwise.Api, which stores them in a SQLite database.

## Projects

| Project | Content |
|---|---|
| `src/Elwise.Core` | Models, the cheapest-window calculation, Danish time and the `IPriceSource` interface |
| `src/Elwise.Infrastructure` | `EnergiDataServiceClient` and `ElwiseApiClient` |
| `src/Elwise.Api` | ASP.NET Core Minimal API with SQLite (EF Core) and an admin endpoint protected by a PBKDF2 password hash |
| `src/Elwise.App` | The .NET MAUI app for Android, iOS and Windows |
| `tests/Elwise.Tests` | xUnit tests for Core, Infrastructure and the server |

## Requirements

- .NET SDK 10.0.402 or newer (see `global.json`)
- The .NET MAUI workload: `dotnet workload install maui`
- Visual Studio on Windows for Android and Windows
- A Mac with Xcode for iOS

## Run the server

1. Make a hash of the admin password and save it with user secrets. The password and the hash are never stored in Git.

   ```
   dotnet run --project src/Elwise.Api -- hash "your-password"
   dotnet user-secrets set "Admin:PasswordHash" "<the hash>" --project src/Elwise.Api
   ```

2. Start the server with F5 in Visual Studio or:

   ```
   dotnet run --project src/Elwise.Api
   ```

   The server runs on `https://localhost:7140`. The database file `elwise.db` is created the first time the server starts.

3. Try the endpoints with `src/Elwise.Api/Elwise.Api.http`:

   | Endpoint | What it does |
   |---|---|
   | `GET /api/prices?area=DK1&firstDay=2026-11-18&dayCount=2` | Returns the prices for one or two days. Days that are not in the database are fetched from Energi Data Service and saved first |
   | `DELETE /api/admin/prices` | Deletes all saved prices. Needs the admin password in the `X-Admin-Password` header |

## Run the app

The phones reach the server on the PC through a Visual Studio dev tunnel. The tunnel address is set in `ElwiseApiAddress` in `src/Elwise.App/MauiProgram.cs`.

1. Start Elwise.Api with the dev tunnel selected in Visual Studio.
2. Start Elwise.App with Debug → Start New Instance.

If the server cannot be reached, the app gets the prices directly from Energi Data Service.

## Tests

```
dotnet test tests/Elwise.Tests
```

The tests run without internet, because they use fixed prices, a fixed clock and fake HTTP responses.
