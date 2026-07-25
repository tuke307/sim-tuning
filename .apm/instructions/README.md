# Instructions

Instructions are **always-on** rules loaded into every conversation automatically.

- Applied regardless of what the user is asking
- Should be short — they consume context budget permanently
- Use for: coding standards, naming conventions, standing constraints

Each `.instructions.md` file is one instruction set.

**When to add an instruction vs a skill:**
Add an instruction only if the rule applies to *every* task in this project. If it is only relevant when working on a specific tool or task, put it in `.apm/skills/` instead.

## Current instructions

| File | Applies to |
|---|---|
| `nestjs.instructions.md` | TypeScript, JavaScript, JSON, and test files |
| `reactjs.instructions.md` | React components, hooks, and styles |
| `security-and-owasp.instructions.md` | All files |
| `typescript-5-es2022.instructions.md` | TypeScript source files |
| `typescript.instructions.md` | Repository-wide TypeScript guidance |
