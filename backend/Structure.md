## Tarot.sln / Tarot.slnx (-x - newer version) -> SOLUTION FILE
= a container/workspace for multiple .NET projects.

.NET solution -> contains several C# projects

### Command

**dotnet new sln -n Tarot** -> Create a .NET solution called Tarot.

---
## Tarot.Api.csproj
It says things like:

- This is a C# project
- These are its dependencies/packages
- This is the .NET version
- These other projects does it reference

---
# Overall Structure
```
backend/
  Tarot.sln

  Tarot.Api/
    Program.cs
    Tarot.Api.csproj

  Tarot.Domain/
    TarotCard.cs
    TarotDeck.cs
    DrawnCard.cs
    ReadingCard.cs
    SpreadDefinition.cs
    SpreadPositionDefinition.cs
    CardOrientation.cs
    Tarot.Domain.csproj

  Tarot.Application/
    Cards/
      ICardDrawingService.cs
      CardDrawingService.cs
    Readings/
      IReadingService.cs
      CreateReadingService.cs
      CreateReadingCommand.cs
      CreateReadingResult.cs
    Spreads/
      ISpreadCatalog.cs
      BuiltInSpreadCatalog.cs
    Tarot.Application.csproj

  Tarot.Infrastructure/
    Tarot.Infrastructure.csproj

  Tarot.Tests/
    Cards/
    Readings/
    Spreads/
    Tarot.Tests.csproj
```

---
## Tarot.Api - the actual web API

It contains things like:
```
HTTP endpoints
request/response DTOs
authentication setup
dependency injection setup
CORS
logging
health checks
```

### Command

**dotnet run --project Tarot.Api** -> project that listens for HTTP requests.

### Tarot.Api/Program.cs
-> file that starts the web app

---
## Tarot.Domain - core business objs

It contains the pure concepts of your app, like:
```
TarotCard
ReadingSession
DrawnCard
CardOrientation
Spread
SpreadPosition
```

---
## Tarot.Application - use cases

It contains the actions your app can perform, like:
```
CreateReading
GetReadingHistory
SaveReading
FavoriteReading
ShareReading
```
A use case is something the user wants to do.

Example:

Create a new tarot reading using a selected spread

The application layer coordinates the logic:

```
Validate the request
Find the selected spread
Ask the card drawing service to draw the required number of cards
Assign each card to its spread position
Later: ask the database to save the session
Later: ask the AI service to interpret the already-drawn cards
Return the final result
```

---
## Tarot.Infrastructure - db + external projects

It contains the technical details, like:
```
Entity Framework Core
PostgreSQL database access
Calling the Python AI service
Redis
Email provider later
File storage later
```
In your app, this project will eventually contain:
```
TarotDbContext
EF Core entity mappings
AI service HTTP client
PostgreSQL repositories
```


---
## Tarot.Tests - automated tests

It contains tests, like:
```
Card drawing should not draw the same card twice.
Each built-in spread should return the expected number of cards.
The backend should reject an empty question.
Reversed cards should use reversed meanings.
```

# Visual version

```
        Tarot.Api
        /       \
       v         v
Application   Infrastructure
       \         /
        v       v
          Domain
```


--------------------------------------------
In .NET, you install packages with:

**dotnet add package PackageName**

Example:

dotnet add Tarot.Infrastructure/Tarot.Infrastructure.csproj package Microsoft.EntityFrameworkCore

This adds Entity Framework Core to the infrastructure project.

## Entity Framework Core is the .NET ORM.

ORM -> It maps C# classes to database tables.

Example:
```
public class TarotCard
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
```

can become a PostgreSQL table like:
```
CREATE TABLE tarot_cards (
    id uuid PRIMARY KEY,
    name text NOT NULL
);
```


Entity Framework Core (EF Core), is how your C# backend talks to PostgreSQL in a clean way.

Example:
```
dbContext.TarotCards.Add(card);
await dbContext.SaveChangesAsync();
```

## Migrations
= version-controlled db changes 

EF Core generate migration files.

## Serilog
= logging library

## FluentValidation
= validates incoming requests

## DLL files
In .NET, when you build a C# project, your .cs source files are compiled into an output file, often a .dll.

The computer does not directly run your .cs files.
When you run:
```
dotnet build
```
.NET compiles your code and produces files such as:
```
bin/Debug/net9.0/AiClicker.Api.dll
```
Then, when you run the app, .NET can execute that .dll.