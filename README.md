# CineBook

A movie ticket booking system in two parts:

- **CineBook.Api**: an ASP.NET Core 8 Web API (JWT login, roles, EF Core, SQLite).
- **CineBook.Desktop**: a WPF desktop client (MVVM) that talks to the API.

Customers can register, browse movies, pick showtimes, choose seats on a live seat map,
book them, and cancel later. Admins manage movies and shows through Swagger.

## Tech

| Part | What it uses |
| --- | --- |
| API | ASP.NET Core 8, Entity Framework Core, SQLite, JWT bearer auth, BCrypt, Swagger |
| Desktop | WPF, XAML, MVVM (hand-written commands and base class), HttpClient |

## Run it

You need the .NET 8 SDK and Windows (WPF is Windows-only). The API itself runs anywhere.

```bash
# Terminal 1: start the API (creates cinebook.db and demo data on first run)
cd src/CineBook.Api
dotnet run

# Terminal 2: start the desktop app
cd src/CineBook.Desktop
dotnet run
```

Or open `CineBook.sln` in Visual Studio 2022, set both projects as startup projects, and press F5.

The API listens on `http://localhost:5080`. Swagger is at `http://localhost:5080/swagger`.

## Demo accounts

| Role | Email | Password |
| --- | --- | --- |
| User | user@cinebook.com | User@123 |
| Admin | admin@cinebook.com | Admin@123 |

## Features

- Register and sign in (passwords hashed with BCrypt, JWT tokens)
- Browse movies and upcoming showtimes
- Interactive seat map: free, your pick, and taken seats
- Book up to 6 seats at once; double booking is blocked by a unique database index
- View and cancel your own bookings (only before the show starts)
- Admin-only endpoints to add or delete movies and add shows (use Swagger's Authorize button)

## API overview

| Method | Route | Access |
| --- | --- | --- |
| POST | /api/auth/register | Public |
| POST | /api/auth/login | Public |
| GET | /api/movies, /api/movies/{id} | Public |
| POST, DELETE | /api/movies | Admin |
| GET | /api/shows/movie/{movieId} | Public |
| GET | /api/shows/{id}/seats | Public |
| POST | /api/shows | Admin |
| POST | /api/bookings | Signed in |
| GET | /api/bookings/mine | Signed in |
| DELETE | /api/bookings/{id} | Signed in (own bookings only) |

## Project layout

```
src/
  CineBook.Api/            Controllers, Models, Dtos, Data (DbContext + seeder), Services
  CineBook.Desktop/        Core (MVVM helpers), Models, Services (ApiClient), ViewModels, Views
```

## Notes

- The JWT secret in `appsettings.json` is for development only. Use user secrets or an
  environment variable in any real deployment.
- Ideas to extend it: unit tests with xUnit, an admin screen in the desktop app,
  payment simulation, email confirmation, and moving to SQL Server.
