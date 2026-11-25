# ParcelTrack

CWEB2302 Web Development II, Common Coursework (Individual).

## Run it
1. Open `ParcelTrack.csproj` in Visual Studio.
2. Package Manager Console: `Update-Database`.
3. F5 (the **http** profile, http://localhost:5420), then open `/parcels`. Five parcels show.

## Demo accounts (made when the app starts)
| Email | Password | Role |
|---|---|---|
| admin@parceltrack.test | Pass123! | Admin |
| staff@parceltrack.test | Pass123! | Staff |

## My work

### Part A – Tokens for the API
- `Jwt` section in `appsettings.json` (Key, Issuer, Audience).
- JWT Bearer configured in `Program.cs` (issuer, audience, expiry, signing key).
- `POST /api/auth/login` issues a 60-minute token that includes the user's roles.
- Wrong password or unknown email → 401 with the same message; invalid email format → 400.
- `[AllowAnonymous]` removed from POST, PUT and DELETE.
- GETs stay open. POST/PUT require Admin or Staff (`CanManageParcels`). DELETE requires Admin.
- No token on a write → 401 (not 302). Staff on DELETE → 403.

### Part B – Request log middleware
- `RequestLogMiddleware` in the `Middleware` folder.
- Adds a short `X-Request-Id` header before the request is passed on.
- Writes one console line: `[id] METHOD path -> status in N ms`.
- Registered after `UseStaticFiles`, before `UseRouting` / `UseAuthentication`.

### Part C – RequireClient action filter
- `[RequireClient]` inherits `ActionFilterAttribute`, applied to POST, PUT and DELETE only.
- Missing or empty `X-Client` → 400 with the specified message; action does not run.
- With a valid header, logs before and after the action.

### Part D – Delivery fee service
- `IDeliveryFeeCalculator` / `DeliveryFeeCalculator` in the `Services` folder.
- Rule: J$500 + J$150 × ceil(weight kg) + J$300 if the branch is not in Kingston or St Andrew.
- Registered as **Scoped** in `Program.cs` and injected into both `ParcelsController` and `ParcelsApiController`.
- Lifetime choice: Scoped — one instance per HTTP request, which matches the request-scoped `DbContext` and avoids sharing state across concurrent requests while still allowing a single calculation object for the whole request.
- Fee shown on `/parcels/{id}` and as `deliveryFee` on every API parcel reply.
- Seeded fees: 800, 650, 1250, 2000, 1250.

### Part E – Tracking code validation
- `[TrackingCode]` attribute in the `Validation` folder, inherits `ValidationAttribute`.
- Must be `PT-` followed by exactly 6 digits. Empty values pass (left to `[Required]`).
- Applied on `TrackingCode` in both `Parcel` and `ParcelInput`.

### Part F – .http file
- `ParcelTrack.http` with `@host`, `@staff_token`, `@admin_token` and the 10 requests in order, each with its expected status.

### Part G
- Targets `net8.0`. No new migration required.
- Demo accounts still created on startup.
- All parts A–F implemented as specified.
