# Entity Map
> Reflects actual entities found in `Competition.Domain/Entities/`.
> Update this file when adding or modifying entities.
> Minimum required for 3-member group: **16 entities**.

---

## Entity Count
| Status | Count |
|---|---|
| Concrete domain entities in current codebase (including lookups) | 24 |
| Required minimum | 16 |
| Still needed | 0 |

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

## Match (Owner: Muhamed)

### MatchStatus
```
MatchStatus : BaseEntity {
  Name          : string (required, max 50)   // "Scheduled" | "Live" | "Finished" | "Postponed" | "Cancelled"
  Slug          : string? (unique index, nullable filter)

  // Navigation
  Matches       : ICollection<Match>
}
```
**Location:** `Competition.Domain/Entities/MatchEntities/MatchStatus.cs` | **EF config:** `AppDbContext.OnModelCreating` | **Table:** `MatchStatuses`

---

### Match
```
Match : BaseEntity {
  Date          : Instant   (required — NodaTime, maps to timestamptz)
  FollowersCount: int       (default 0)
  HomeScore     : int?
  AwayScore     : int?
  HalfHomeScore : int?
  HalfAwayScore : int?
  IsPostponed   : bool      (default false)
  Slug          : string?   (unique index, nullable filter)

  // FKs
  StatusId      : Guid      (FK → MatchStatus, Restrict)
  StadiumId     : Guid      (FK → Stadium, Restrict)

  // Navigation
  Status        : MatchStatus?
  Stadium       : Stadium?
}
```
**Location:** `Competition.Domain/Entities/MatchEntities/Match.cs` | **EF config:** `AppDbContext.OnModelCreating` | **Table:** `Matches`
**Soft delete:** `HasQueryFilter(x => x.DeletedAt == null)` applied (first entity in project to use EF-level filter)

---

## Club and Planned Entities (Owner: Dino)

### Club — COMP-13 (Owner: Dino)
```
Club {
  Id          : Guid (internal PK)
  PublicId    : string (32 chars, unique, inherited)
  Name        : string (required, max 100)
  Slug        : string (required, max 150, unique among active clubs)
  FoundingDate: LocalDate (required, not in future)
  ClubAge     : ClubAge (U7, U9, U11, U13, U16, U19, U21, Senior)
  CityId      : Guid (required FK → City)
  StadiumId   : Guid (required FK → Stadium, must belong to City)

  // Navigation
  Stadium     : Stadium
  City        : City (virtual)
}
```
**Location:** `Competition.Domain/Entities/Club.cs` | **EF:** `AppDbContext.OnModelCreating`, Restrict FKs, soft-delete query filter.
Selection relationships are tracked separately in COMP-36. Public API uses public IDs; inherited DeletedAt is an Instant? in the current code.

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

## Entity Count After COMP-13
The current base contains 23 concrete domain entities; Club adds one, bringing the count to 24. This excludes abstract BaseEntity/Person and the Identity ApplicationRole class. Simple lookups are included in this inventory count; even excluding MatchStatus, FormationMark, Position and SubPosition leaves 20 entities, above the RS1 minimum of 16. Earlier planned entries outside Club are historical and should be reconciled by their feature owners.
