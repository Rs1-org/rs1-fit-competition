# Entity Map
> Reflects actual entities found in `Competition.Domain/Entities/`.
> Update this file when adding or modifying entities.
> Minimum required for 3-member group: **16 entities**.

---

## Entity Count
| Status | Count |
|---|---|
| Confirmed in codebase | 13 |
| Required minimum | 16 |
| Still needed | 3 |

---

## Geo (Owner: Muhamed)

### Country
```
Country {
  Id          : int (PK)
  Name        : string (required)
  ISOCode     : string (2 chars, unique)
  Flag        : string (URL, nullable)

  // Navigation
  Cities      : ICollection<City>
}
```
**Location:** `Competition.Domain/Entities/` | **EF config:** `AppDbContext.OnModelCreating`

---

### City
```
City {
  Id          : int (PK)
  Name        : string (required)
  ZIP         : string (nullable)
  CountryId   : int (FK → Country)

  // Navigation
  Country     : Country
  Stadiums    : ICollection<Stadium>
}
```
**Location:** `Competition.Domain/Entities/` | **EF config:** `AppDbContext.OnModelCreating`

---

### GeoData
```
GeoData {
  // Exact fields TBC — entity exists in codebase, details to be confirmed
}
```
> ⚠️ Confirm fields and purpose — add to this doc when clarified.

---

### Stadium
```
Stadium {
  Id          : int (PK)
  Name        : string (required)
  Capacity    : int (> 0)
  Address     : string (nullable)
  CityId      : int (FK → City)

  // Navigation
  City        : City
}
```
**Location:** `Competition.Domain/Entities/MatchEntities/Stadium.cs` | **EF config:** `AppDbContext.OnModelCreating`

---

## Persons (Owner: Edin) — Base class: `Person`

### Person (abstract base)
```
Person {
  Id              : int (PK)
  FirstName       : string (required)
  LastName        : string (required)
  BirthDate       : DateOnly
  ProfileImage    : string (nullable)
  // + BaseEntity fields (DeletedAt for soft delete)
}
```

---

### Player : Person
```
Player {
  // inherits Person
  Height          : decimal (nullable)
  Weight          : decimal (nullable)
  Position        : string or enum
  PreferredFoot   : string or enum
}
```
**Location:** `Competition.Domain/Entities/Person/Player.cs`

---

### Referee : Person
```
Referee {
  // inherits Person
  ExperienceYears : int
  LicenseType     : string or enum
}
```
**Location:** `Competition.Domain/Entities/Person/Referee.cs`

---

### Manager : Person
```
Manager {
  // inherits Person
  CoachingLicense : string (nullable)
  Philosophy      : string (nullable)
}
```
**Location:** `Competition.Domain/Entities/Person/Manager.cs`

---

## History / Tracking (Owner: Edin / Shared)

### PlayerSelectionHistory
```
PlayerSelectionHistory {
  Id          : int (PK)
  PlayerId    : int (FK → Player)
  // ClubId or SelectionId (TBC)
  From        : DateTime
  To          : DateTime (nullable)
}
```

---

### SelectionManagerHistory
```
SelectionManagerHistory {
  Id          : int (PK)
  ManagerId   : int (FK → Manager)
  SelectionId : int (FK → Selection — planned)
  From        : DateTime
  To          : DateTime (nullable)
}
```

---

### TransferValueHistory
```
TransferValueHistory {
  Id          : int (PK)
  PlayerId    : int (FK → Player)
  Value       : decimal
  Date        : DateTime
}
```

---

### MandatDate
```
MandatDate {
  Id          : int (PK)
  StartDate   : DateTime
  EndDate     : DateTime (nullable)
  // Generic mandate tracking — confirm FK targets
}
```

---

## Security (Owner: Shared)

### ApplicationUser
```
ApplicationUser : IdentityUser {
  FirstName       : string
  LastName        : string
  ProfileImage    : string (nullable)

  // Navigation
  RefreshTokens   : ICollection<RefreshToken>
}
```

---

### RefreshToken
```
RefreshToken {
  Id          : int (PK)
  Token       : string (unique)
  Expiry      : DateTime
  UserId      : string (FK → ApplicationUser)

  // Navigation
  User        : ApplicationUser
}
```

---

## Planned Entities (Owner: Dino) — NOT YET IMPLEMENTED

### Club ❌
```
Club {
  Id          : int (PK)
  Name        : string (required)
  Logo        : string (URL, nullable)
  StadiumId   : int (FK → Stadium)

  // Navigation
  Stadium     : Stadium
  Players     : ICollection<Player>
}
```

---

### Selection ❌
```
Selection {
  Id          : int (PK)
  Name        : string (required)
  CountryId   : int (FK → Country)
  // + additional fields TBD
}
```

---

### Match ❌
```
Match {
  Id              : int (PK)
  HomeTeamId      : int (FK → Club or Selection)
  AwayTeamId      : int (FK → Club or Selection)
  StadiumId       : int (FK → Stadium)
  ScheduledAt     : DateTime
  // + Status, Score fields TBD
}
```

---

### MatchEvent ❌
```
MatchEvent {
  Id          : int (PK)
  MatchId     : int (FK → Match)
  Type        : enum (Goal | Card | Substitution)
  Minute      : int (0–120)
  // + PlayerId FK TBD
}
```

---

## BaseEntity (all entities inherit this)
```
BaseEntity {
  DeletedAt   : DateTime? (null = active, set = soft deleted)
}
```
**Location:** `Competition.Domain/Entities/BaseEntity.cs`

---

## Relationship Summary
```
Country     ──< City
City        ──< Stadium
Person      <|— Player
Person      <|— Referee
Person      <|— Manager
Player      ──< PlayerSelectionHistory
Player      ──< TransferValueHistory
Manager     ──< SelectionManagerHistory
ApplicationUser ──< RefreshToken

// Planned (Dino):
Club        ──< Player
Club        >── Stadium
Match       ──< MatchEvent
```

---

## Missing Entities to Reach 16 Minimum
Current confirmed: **13** (Country, City, GeoData, Stadium, Player, Referee, Manager,
PlayerSelectionHistory, SelectionManagerHistory, TransferValueHistory, MandatDate,
ApplicationUser, RefreshToken)

Dino's planned additions: Club, Selection, Match, MatchEvent = **+4 → total 17** ✅

> Once Dino implements his entities the minimum will be met.