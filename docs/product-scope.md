# Product Scope

## Purpose

OceanCare helps users identify marine life with natural-language search and gives administrators a secured interface to manage the searchable catalog.

## Primary users

- **Researchers and enthusiasts** searching marine species by descriptive text
- **Administrators** curating species, categories, and dynamic attributes

## Supported use cases

- Search marine species by descriptive phrases and see ranked matches in real time
- Browse species and categories through the public API
- Authenticate as an admin and manage categories, attributes, and species records
- Regenerate search embeddings automatically when species data changes

## Out of scope for this slice

- External identity providers
- Production-grade embedding providers beyond the built-in TF-IDF plugin
- Bulk import workflows
- Native mobile clients
