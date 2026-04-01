<!--
Sync Impact Report
Version change: N/A (template) -> 1.0.0
Modified principles:
- Template Principle 1 -> I. Training-First Scope
- Template Principle 2 -> II. Security by Default (Training Grade)
- Template Principle 3 -> III. Spec-Driven, Testable Delivery
- Template Principle 4 -> IV. Offline-First with Replaceable Infrastructure
- Template Principle 5 -> V. Simplicity, Clarity, and Maintainability
Added sections:
- Technical Guardrails
- Delivery Workflow and Quality Gates
Removed sections:
- None
Templates requiring updates:
- ✅ .specify/templates/plan-template.md
- ✅ .specify/templates/spec-template.md
- ✅ .specify/templates/tasks-template.md
- ✅ .specify/templates/constitution-template.md (already aligned as base template)
- ⚠ pending .specify/templates/commands/*.md (directory not present; no files to update)
Follow-up TODOs:
- None
-->

# ContosoDashboard Constitution

## Core Principles

### I. Training-First Scope
The repository MUST remain fit for guided training and local practice, not production deployment.
New features MUST keep setup simple, deterministic, and runnable without paid cloud dependencies.
Any production-oriented pattern included for learning MUST be explicitly labeled as guidance and paired with training-safe defaults.

Rationale: The project exists to teach spec-driven development in a constrained environment.

### II. Security by Default (Training Grade)
All user-facing and service-level changes MUST preserve authentication and authorization
boundaries already established in the application. 
Protected routes and data access paths MUST enforce role- and user-scoped checks, and changes MUST avoid introducing IDOR-style access patterns. 
Security-relevant behavior MUST be documented in the feature spec and validated in acceptance scenarios.

Rationale: Even training projects must model safe defaults and prevent unsafe habits.

### III. Spec-Driven, Testable Delivery
Every feature MUST originate from a written spec with independently testable user stories.
Plans and tasks MUST preserve story-level independence and explicit acceptance criteria.
When tests are requested or risk is high, implementation MUST include tests that fail before code changes and pass after completion.

Rationale: SDD only works when requirements and verification are traceable through delivery.

### IV. Offline-First with Replaceable Infrastructure
Core workflows MUST function in a local/offline training environment. Infrastructure integrations (storage, identity, external APIs) MUST be introduced behind interfaces so implementations can be swapped without changing business logic.

Rationale: Students need reliable local execution while learning migration-safe architecture.

### V. Simplicity, Clarity, and Maintainability
Design and implementation MUST prefer the simplest approach that satisfies current requirements.
Developers MUST avoid speculative abstractions, hidden coupling, and broad refactors unrelated to the active story. Documentation and code organization MUST keep learning intent obvious.

Rationale: The codebase is a teaching tool; readability and focused change scope are critical.

## Technical Guardrails

- Primary implementation stack is ASP.NET Core 8.0 with Blazor Server and EF Core.
- Authentication in this repository is mock/cookie-based for training; production identity guidance MUST remain clearly marked as non-default.
- Database and storage choices for training features MUST run locally by default.
- New external dependencies MUST be justified in plan.md and compatible with offline training use.

## Delivery Workflow and Quality Gates

1. Define feature scope in spec.md with prioritized user stories, edge cases, and measurable outcomes.
2. Create plan.md that passes Constitution Check gates before implementation starts.
3. Generate tasks.md grouped by user story, preserving independent delivery and validation.
4. Implement in small increments, validating authorization boundaries and user isolation for each impacted flow.
5. Update documentation when behavior, security assumptions, or infrastructure abstractions change.

## Governance

This constitution is authoritative for feature specification, planning, and task generation in this repository.

- Amendment Process: Propose changes via PR that includes rationale, impacted templates/docs, and a Sync Impact Report.
- Versioning Policy: Semantic versioning applies.
  - MAJOR: Principle removal/redefinition or governance changes that alter compliance meaning.
  - MINOR: New principle/section or materially expanded mandatory guidance.
  - PATCH: Clarifications, wording improvements, typo fixes, and non-semantic refinements.
- Compliance Review: Every plan and review MUST include an explicit Constitution Check. Violations MUST be documented with justification and approved before implementation.
- Runtime Guidance: README.md and relevant docs MUST stay aligned with constitutional principles.

**Version**: 1.0.0 | **Ratified**: 2026-04-01 | **Last Amended**: 2026-04-01
