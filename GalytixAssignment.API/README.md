# GalytixAssignment.API

This project is a small ASP.NET Core Web API built for calculating average GWP values by country and line of business. It reads source data from a CSV file, validates incoming requests, calculates the averages, and returns the result as a simple JSON response.

## Tech stack

- .NET 10
- ASP.NET Core Web API
- FluentValidation
- Swagger / OpenAPI
- In-memory caching
- xUnit for tests

## What the API does

The API exposes a single endpoint that accepts:

- a `country`
- a list of lines of business (`lob`)

It then loads the CSV dataset, filters the records for the requested country and lines of business, calculates the average GWP, and returns the results.

### Endpoint

`POST /server/api/gwp/avg`

### Example request

```json
{
  "country": "ae",
  "lob": ["transport", "property"]
}
```

### Example response

```json
[
  {
	"lineOfBusiness": "transport",
	"averageGwp": 12.34
  },
  {
	"lineOfBusiness": "property",
	"averageGwp": 45.67
  }
]
```

## Project structure

```text
GalytixAssignment.API/
├── Controllers/
│   └── CountryGwpController.cs
├── Data/
│   └── gwpByCountry.csv
├── Exceptions/
│   ├── ApiException.cs
│   ├── DataAccessException.cs
│   └── RequestProcessingException.cs
├── Middleware/
│   └── GlobalExceptionHandler.cs
├── Models/
│   ├── AverageGwpResponse.cs
│   ├── CountryGwpData.cs
│   └── CountryGwpRequest.cs
├── Repositories/
│   ├── CountryGwpRepository.cs
│   └── ICountryGwpRepository.cs
├── Services/
│   ├── CountryGwpService.cs
│   └── ICountryGwpService.cs
├── Validators/
│   └── CountryRequestValidator.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### Folder overview

- **Controllers**: Defines the HTTP endpoint and request handling logic.
- **Data**: Contains the source CSV file used by the API.
- **Exceptions**: Custom exception types used for clearer error handling.
- **Middleware**: Centralized exception handling for consistent error responses.
- **Models**: Request, response, and internal data models.
- **Repositories**: Responsible for reading and parsing the CSV data.
- **Services**: Contains the business logic for filtering data and calculating averages.
- **Validators**: FluentValidation rules for incoming requests.
- **Program.cs**: Application bootstrap and dependency registration.

## How to run locally

### Prerequisites

Make sure you have the following installed:

- .NET 10 SDK
- Visual Studio 2026 or later, or the `dotnet` CLI

### Run with Visual Studio

1. Open the solution in Visual Studio.
2. Set `GalytixAssignment.API` as the startup project.
3. Run the project.
4. Run the http profile the browser should open automatically at http://localhost:9091.
4. Access http://localhost:9091/swagger/index.html to access swagger documentation page.

### Run with the .NET CLI

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project .\GalytixAssignment.API\GalytixAssignment.API.csproj
```

## Local URLs

Based on the current launch settings, the API runs on:

- `http://localhost:9091`
- `https://localhost:722`

If you run in Development mode, Swagger should be available at:

- `http://localhost:9091/swagger`
- `https://localhost:7224/swagger`

The OpenAPI document is also exposed in Development mode.

## Running tests

The solution also includes a test project: `GalytixAssignment.Test`.

Run tests from the repository root with:

```powershell
dotnet test
```

## Notes

- The CSV file is loaded from `Data/gwpByCountry.csv` at runtime.
- Request validation is handled with FluentValidation before business logic runs.
- Responses are cached in memory to avoid recalculating the same country and line-of-business combination repeatedly.
- Custom exception handling is wired through the global exception handler for cleaner API responses.
- Each service, repository, validators and controller is designed to be single responsibility for better maintainability and testability.
