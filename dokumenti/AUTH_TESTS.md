# Auth unit test evidence (COMP-85)

These tests cover Dino's authentication area. They run without a database using
xUnit, Moq and FluentAssertions, following `BaseUnitTest`.

| Test class | Behavior covered |
| --- | --- |
| `RegisterUserValidatorTests` | Valid registration, invalid email, weak password, confirmation mismatch, duplicate email, blank names/phone |
| `LoginUserHandlerTests` | Wrong password issues no tokens, successful login returns tokens/roles, request cancellation |
| `RefreshTokenHandlerTests` | Expired/revoked token rejection, successful rotation, cancellation passed to persistence/revocation, logout |

All classes are in `backend/CompetitionSolution/Competition.Tests/UnitTests/Auth/`.
For the review form, two concrete examples are
`RegisterUserValidatorTests.ExistingEmail_IsRejectedBeforeCreatingAUser` and
`RefreshTokenHandlerTests.RevokedOrExpiredToken_IsRejectedWithoutIssuingNewTokens`.
Record actual authorship and evidence according to the course's rules.

```powershell
dotnet test backend/CompetitionSolution/Competition.Tests/Competition.Tests.csproj --configuration Release --filter "FullyQualifiedName~RegisterUserValidatorTests|FullyQualifiedName~LoginUserHandlerTests|FullyQualifiedName~RefreshTokenHandlerTests"
```

HTTP validation, token contracts and database-backed behavior are separately covered
by `IntegrationTests/AuthValidationTests.cs`, `DeploymentTests.cs` and `SeededLoginTests.cs`.
