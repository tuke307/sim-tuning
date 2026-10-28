---
paths:
  - "**"
---

# Git Commit Instructions

This project enforces commit message rules via **commitlint** (checked by the `commit-msg` husky hook) and code quality via **lint-staged** (checked by the `pre-commit` hook).

---

## Commit Message Format

```
<type>(<optional scope>): <subject>

<optional body>

<optional footer>
```

Every part has strict rules — violations block the commit.

---

## Types (required, lowercase)

Use exactly one of:

| Type | When to use |
|------|-------------|
| `feat` | New feature or capability |
| `fix` | Bug fix |
| `refactor` | Code change that is neither a fix nor a feature |
| `style` | Formatting, whitespace, missing semicolons — no logic change |
| `perf` | Performance improvement |
| `test` | Adding or updating tests |
| `docs` | Documentation only |
| `build` | Build system, dependencies, tooling |
| `ci` | CI/CD configuration |
| `chore` | Maintenance tasks that don't fit above |
| `revert` | Reverting a previous commit |

---

## Rules enforced by commitlint

| Rule | Requirement |
|------|-------------|
| `type-enum` | Type must be one of the 11 types above |
| `type-case` | Type must be **lowercase** |
| `type-empty` | Type is **required** |
| `subject-empty` | Subject (after the colon) is **required** |
| `subject-full-stop` | Subject must **not** end with a `.` |
| `subject-case` | Subject must **not** be Sentence-case, Start-Case, PascalCase, or UPPER-CASE — use lower-case or plain prose |
| `header-max-length` | Entire first line ≤ **100 characters** |
| `body-leading-blank` | If a body is present, leave **one blank line** between header and body |
| `body-max-line-length` | Each body line ≤ **100 characters** |
| `footer-leading-blank` | If a footer is present, leave **one blank line** between body and footer |
| `footer-max-line-length` | Each footer line ≤ **100 characters** |

---

## Scope (optional)

Scope is free-form and describes what area of the codebase was changed.

```
feat(nav): add sticky CTA button
fix(a11y): correct focus ring on input fields
fix(design-tokens): bump muted-foreground contrast
```

Common scopes in this repo: `nav`, `ui`, `ux`, `a11y`, `api`, `auth`, `admin`, `db`, `email`, `jobs`, `analytics`, `design-tokens`, `deps`.

---

## Subject

- Start with a **verb in imperative mood**: `add`, `fix`, `remove`, `update`, `extract`, `rename`
- No capital first letter (lower-case or plain prose only)
- No trailing period
- ≤ 100 characters total for the entire header line

```
# Good
feat(nav): add skip-to-content link for keyboard navigation
fix(ux): show service descriptions on mobile without hover

# Bad — subject starts with capital → blocked by subject-case rule
feat(nav): Add skip-to-content link
# Bad — trailing period → blocked by subject-full-stop rule
fix(ux): show service descriptions on mobile.
# Bad — type not in enum → blocked by type-enum rule
improvement(nav): add skip link
```

---

## Body (optional)

Explain **why**, not what. Use when the commit isn't self-evident.

- Separate from header with **one blank line**
- Wrap at **100 characters per line**

```
fix(ux): show welcome intro once per session

The 4.5s intro was replaying on every navigation including back
navigation from forms. sessionStorage guard ensures it only shows
once per browser session, resetting when the tab is closed.
```

---

## Footer (optional)

Used for breaking changes, issue references, or co-author credits.

- Separate from body (or header if no body) with **one blank line**
- Wrap at **100 characters per line**

```
feat(auth): replace session tokens with JWT

BREAKING CHANGE: existing sessions are invalidated on deploy.

Closes #42
Co-Authored-By: Claude Sonnet 4 <noreply@anthropic.com>
```

---

## Pre-commit: lint-staged

The `pre-commit` hook runs lint-staged on staged files before the commit is created.

| Glob | Commands run |
|------|-------------|
| `apps/**/*.{js,ts,tsx}` | `prettier --write .` then `eslint --fix` |
| `packages/**/*.{js,ts,tsx}` | `prettier --write .` then `eslint --fix` |
| `*.json` | `prettier --write .` |

**Always stage specific files** (`git add path/to/file`) rather than `git add -A` to avoid accidentally staging unrelated changes that then fail linting.

---

## HEREDOC pattern (required for multi-line messages)

Always pass multi-line commit messages via a heredoc to preserve formatting:

```bash
git commit -m "$(cat <<'EOF'
feat(nav): add active state indicator to full-screen menu

usePathname drives a cobalt left-border on the current page link
so users always know where they are in the navigation.

Co-Authored-By: Claude Sonnet 4 <noreply@anthropic.com>
EOF
)"
```

---

## Quick reference — valid examples

```bash
# Minimal (type + subject only)
git commit -m "fix(ui): correct hero height on desktop"

# With body
git commit -m "$(cat <<'EOF'
refactor(nav): extract ContactInfoBlock to remove duplicated markup

Mobile accordion and desktop grid previously contained identical
contact info JSX. Shared component eliminates the maintenance risk.
EOF
)"

# With breaking change footer
git commit -m "$(cat <<'EOF'
feat(auth): switch to JWT-based sessions

BREAKING CHANGE: cookie-based sessions no longer valid after deploy.
EOF
)"
```
