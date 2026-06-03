# Anemoi HR AI Guidelines

This folder contains AI-ready engineering guidelines and phase prompts for implementing the HR Platform inside the Anemoi ecosystem and the cody-web-app frontend.

## How to use with Codex / Cursor / Claude Code

Attach or reference these files in this order:

1. `shared/00-backend-architecture-guide.md`
2. `shared/01-frontend-guidelines.md`
3. `hr-module/00-hr-platform-overview.md`
4. The exact phase prompt under `hr-module/prompts/`

Tell the AI to follow the phase prompt strictly and stop at the required review point.

## Execution rule

For backend implementation, always follow the 3-step execution model from the architecture guide:

1. Domain & Data
2. Application Layer - CQRS & Mappings
3. API & Communication

Do not let the AI implement all layers in one pass unless you explicitly request it.

## Suggested repository folder

```text
docs/
└── ai-guidelines/
    ├── shared/
    └── hr-module/
```

## Conflict priority

If files conflict, use this priority:

```text
ArchitectureGuide.md
> frontend-guidelines.md
> HR module guide
> phase prompt
> ad-hoc user instruction
```
