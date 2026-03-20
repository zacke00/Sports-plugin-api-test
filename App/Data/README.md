# EF Core Migrations

This project uses **EF Core Code-First** migrations. The database schema is defined in `SportsVenuesContext.cs` and entity classes live in the `Entities/` folder. Migrations are applied automatically at application startup.

## Prerequisites

Make sure you have the EF Core CLI tool installed globally:

```bash
dotnet tool install --global dotnet-ef
```

## Creating a New Migration

After making changes to the entity classes or the `SportsVenuesContext` model configuration, generate a new migration from the **solution root**:

```bash
dotnet ef migrations add <MigrationName> --project App --output-dir Data/Migrations
```

**Example:**

```bash
dotnet ef migrations add AddVenueCapacity --project App --output-dir Data/Migrations
```

## Removing the Last Migration

If the latest migration has **not** been applied to a database yet, you can remove it:

```bash
dotnet ef migrations remove --project App
```

## Applying Migrations Manually

Migrations are applied automatically when the app starts (`Database.MigrateAsync()`). If you need to apply them manually (e.g. against a staging database):

```bash
dotnet ef database update --project App
```

## Generating a SQL Script

To generate an idempotent SQL script for production deployments:

```bash
dotnet ef migrations script --idempotent --project App --output migrate.sql
```

## Folder Structure

```
Data/
├── Entities/                  # Entity classes (Fixture, Venue, VenueFixture)
├── Migrations/                # Auto-generated migration files
├── Services/                  # Background services (IndexOptimizationService)
├── SportsVenuesContext.cs     # DbContext with Fluent API configuration
└── README.md                  # This file
```
