# BlazorAPIProject — Herbruikbare API-template

Een schone ASP.NET Core Web API startertemplate (.NET 10) die je kunt klonen als basis voor een nieuwe API. Bevat een werkende gelaagde architectuur met authenticatie, gebaseerd op de technische opzet van FitsyncApi (maar zonder de fitness-specifieke logica).

## Wat zit erin

Gelaagde architectuur met twee projecten:

* **BlazorAPIProject.DataAccess**
  Class library: EF Core, entities, ApplicationDbContext, migrations

* **BlazorAPIProject**
  Web API: controllers, models (commands/responses), mappings, utilities

* Entity Framework Core + SQL Server (LocalDB) met migrations

* JWT Bearer authenticatie
  `POST /api/account/login` geeft een JWT terug

* AutoMapper voor command/entity/response-mapping

* AES HashingHelper voor wachtwoord-encryptie
  `Utilities/HashingHelper.cs`

* Globale exception handler
  Volledige foutdetails in Development, generieke 500 in Production

* OpenAPI / Swagger UI
  Opent automatisch op `/swagger` in Development

* Voorbeelddomein
  Account / Role / AccountRole / Token
  (user management), klaar om uit te breiden

---

## Snel starten

### 1. Database aanmaken / migraties toepassen

```bash
dotnet ef database update --project BlazorAPIProject.DataAccess --startup-project BlazorAPIProject
```

### 2. API draaien

```bash
dotnet run --project BlazorAPIProject
```

Swagger opent op:

```text
https://localhost:7124/swagger
```

---

## Deze template gebruiken voor een nieuw project

1. Kopieer / kloon de map naar een nieuwe projectmap.

2. Hernoem naar wens:

   * map
   * `.csproj`-namen
   * `.slnx`
   * namespace / `RootNamespace`

3. Pas in `BlazorAPIProject/appsettings.json` aan:

**ConnectionStrings:DefaultConnection**

```json
"ConnectionStrings": {
  "DefaultConnection": "jouw-connection-string"
}
```

→ Gebruik hier je eigen databasenaam.

**Authentication:Bearer:SigningKey**

```json
"Authentication": {
  "Bearer": {
    "SigningKey": "jouw-geheime-sleutel"
  }
}
```

→ Gebruik hier een eigen geheime sleutel van minimaal 32 tekens.

**HashingKey:Key**

```json
"HashingKey": {
  "Key": "jouw-geheime-sleutel"
}
```

→ Gebruik hier een eigen geheime sleutel.

4. Voeg je eigen entities toe in:

```text
BlazorAPIProject.DataAccess/Entities
```

5. Registreer ze als `DbSet` in:

```text
ApplicationDbContext
```

6. Maak een nieuwe migratie.

7. Voeg per entity een controller + command/response-modellen toe.
   Kopieer het patroon van:

```text
AccountController
RoleController
```

---

## Belangrijk voor productie

Zet echte geheime sleutels **NIET** in `appsettings.json`.

Gebruik:

* User Secrets
* Environment Variables

De `SigningKey` moet lang en willekeurig zijn.

De standaardwaarden hier zijn alleen placeholders.

---

## Nieuwe migratie / model wijzigen

### Migratie toevoegen na entity-wijziging

```bash
dotnet ef migrations add <Naam> --project BlazorAPIProject.DataAccess --startup-project BlazorAPIProject
```

### Laatste migratie ongedaan maken

> Alleen gebruiken als de migratie nog niet is toegepast op de database.

```bash
dotnet ef migrations remove --project BlazorAPIProject.DataAccess --startup-project BlazorAPIProject
```
