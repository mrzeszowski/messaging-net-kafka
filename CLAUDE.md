# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A chat app demonstrating a transactional outbox + Kafka consumer pipeline in .NET 10:
- `chat-api/` — ASP.NET Core minimal API (`Chat.Api`), a reusable messaging library (`Chat.Messaging`), and integration tests. Solution: `chat-api/Chat.sln`.
- `chat-ui/` — React 19 + TypeScript + Vite + Tailwind front end (shadcn-style components). Talks to the API at `http://localhost:8092` via REST and SignalR (`/chat-hub`). "Login" is just a user stored in `localStorage`.

## Commands

Backend (run from `chat-api/`):
```bash
docker compose up                       # Postgres :5432, Kafka :9092, kafka-ui :8080
dotnet build
dotnet test                             # integration tests; Testcontainers starts its own Postgres + Kafka (Docker required)
dotnet test --filter "FullyQualifiedName~MessageTests.Should_create_message"   # single test
```
From `chat-api/src/Chat.Api/`:
```bash
dotnet ef database update               # apply migrations to local Postgres (database chatdb, schema "chat")
dotnet ef migrations add <Name>         # migrations live in Chat.Api/Migrations
dotnet run --launch-profile http        # Swagger UI at http://localhost:8092
```

Frontend (run from `chat-ui/`): `npm install`, `npm run dev` (http://localhost:5173 — the only origin allowed by the API's CORS policy), `npm run build` (`tsc -b && vite build`), `npm run lint`.

## Architecture: event flow

The message flow spans both projects and is the core of the codebase:

1. **Publish (outbox write)** — `POST /api/v1/messages` (`Program.cs`) calls `IEventPublisher.PublishAsync`, which only *adds* an `EventOutbox` row to the scoped `DbContext`. The caller must call `SaveChangesAsync` so the event commits atomically with domain changes. Note the endpoint deliberately does not persist `Message` directly — the message is stored by the event handler.
2. **Forward** — `EventForwarderBackgroundService` holds a Postgres distributed lock (single active forwarder across instances) and loops `EventForwarder.ForwardAsync`: reads up to 100 outbox rows ordered by `LocalOffset`, produces them to Kafka via `IProducer` (`KafkaProducer`, headers `message.id` / `message.type`, key = event `Id`), then moves them from `EventOutbox` to `EventLog`.
3. **Consume** — `EventDispatchingBackgroundService` calls `ConsumerPool.AdjustConsumers` every 5s, keeping `Subscription.TasksCount` `KafkaConsumerWorker`s alive per subscription (recreating faulted ones). Workers use `EnableAutoCommit=true` + `EnableAutoOffsetStore=false` and call `StoreOffset` only after processing → at-least-once delivery.
4. **Process** — `MessageProcessor` resolves the CLR type from the `message.type` header (full type name) using `TypeRepository`, which scans the assembly passed to `AddMessaging<TAssembly>` (i.e. `Chat.Api`). It builds a per-message DI scope and an `EventPipelineFactory<TEvent>` pipeline: `TransactionFilter` (calls `SaveChangesAsync` after handlers) → `ObservabilityFilter` → all `IEventHandler<TEvent>`. Exceptions are logged and swallowed (no dead-letter yet).
5. **Handle** — e.g. `MessageSentHandler` must be idempotent (at-least-once); it checks for an existing `Message`, adds it (no explicit save — `TransactionFilter` does it), and broadcasts via SignalR `Subscribe`.

Conventions that follow from this:
- Events implement `IEvent` (with `Id`), must live in the `Chat.Api` assembly to be resolvable, and are serialized with `System.Text.Json`.
- Register handlers as scoped `IEventHandler<TEvent>`; don't call `SaveChangesAsync` inside handlers.
- `Chat.Messaging` depends on a non-generic `DbContext` registration — `Chat.Api` maps it to `ChatDbContext`, which also applies the messaging EF configurations (`EventOutbox`, `EventLog`) from the `Chat.Messaging` assembly.
- Topic (`chat.events`), consumer group ID and lock name are hard-coded (marked `todo`).
- `Chat.Messaging` exposes internals to `Chat.Integration.Tests`.

## Tests

`ApiFixture` (shared via an xUnit collection) starts Postgres and Kafka Testcontainers, overrides config through `WebApplicationFactory`, runs migrations, and decorates `IProducer` (Scrutor) with `TestProducerDecorator` to capture produced events in `EventsCollector`. Async assertions use `fixture.RetryPolicy` (Polly, up to 200×200ms); use `ShouldBePublished<T>` for outbound events and `PublishEvent<T>` to inject inbound Kafka events.

## chat-ui conventions (from `chat-ui/.github/copilot-instructions.md`)

- Reusable UI components live in `src/components/ui/` (lowercase filenames), accept `className` + spread `...props`, compose classes with `cn` from `@/lib/utils`, and set `data-slot` attributes.
- App logic is in `src/ChatUi.tsx` (PascalCase for app files). Use `@/` path-alias imports.
