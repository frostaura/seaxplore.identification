# Admin and Data Model

## Admin workflow

1. An admin signs in with username and password.
2. The API returns a JWT scoped to the configured issuer and audience.
3. Authenticated admins manage:
   - categories
   - dynamic attributes
   - species records and their attribute values
4. Species create and update operations regenerate the stored embedding used by search.

## Relational model

- **Category**
  - one-to-many with `Species`
- **Species**
  - belongs to one `Category`
  - has many `SpeciesAttributeValue`
  - has one `SearchEmbedding`
- **MarineAttribute**
  - defines configurable metadata fields such as color or habitat
  - one-to-many with `SpeciesAttributeValue`
- **SpeciesAttributeValue**
  - joins a species to a dynamic attribute and stores the value
- **SearchEmbedding**
  - stores the generated vector for one species
- **Admin**
  - stores a username and BCrypt password hash

## Seeded baseline

Startup seeding creates:

- one default admin account
- four marine categories
- eight reusable attributes
- a starter species catalog with stored embeddings

This seeded baseline is intended for local development and automated validation.
