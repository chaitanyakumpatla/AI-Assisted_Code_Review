# Code Review Checklist — Inventory App

This checklist is tailored to the Angular Inventory app in this repository. Use it to review PRs or perform manual audits.

How to use
- Mark each item: `Pass`, `Needs work`, or `N/A` and add a short note.
- Set a priority label: Critical / High / Medium / Low.
- For each `Needs work` item include a suggested change and one or two test steps.

---

## Summary
- Location: `code-review-checklist-inventory-app.md` (this file)
- CSV/Excel versions are also available: `code-review-checklist-inventory-app.csv` and `code-review-checklist-inventory-app.xml`.

---

## Checklist

| ID | Category | Item | Description | Priority | Test Steps | Notes |
|----|----------|------|-------------|----------|------------|-------|
| 1 | Architecture & Boot | bootstrapApplication usage | Verify app starts with `bootstrapApplication` and `appConfig` providers are correct. | Medium | Start app, ensure no duplicate router/provider errors in console. | |
| 2 | Components | Standalone imports | Confirm standalone components declare required imports (Forms, CommonModule, Router etc.). | Low | Open each standalone component and check `imports` array. | |
| 3 | Routing | Protected routes use `canActivate` | Verify routes that require auth include `canActivate: [authGuard]` and intended role checks. | High | Attempt to access protected route while logged out and expect redirect to `/login`. | Add `data.role` where role-based access is needed. |
| 4 | Guards | `returnUrl` handling | Ensure guard sets `returnUrl` and login flow redirects back to it after successful login. | High | Access `/dashboard` while logged out; after login expect redirect to original URL. | Update `LoginComponent` to honor query param if missing. |
| 5 | Auth & Security | Client-only auth risk | Confirm team understands this is demo-level client-side auth; note production requirements (tokens, backend). | Critical | Review README and security docs; list TODOs for migrating to backend auth. | Blocker for production usage. |
| 6 | Persistence | localStorage safety | Wrap `localStorage` reads/writes in `try/catch` and provide fallback in-memory store. | Critical | Run app in privacy mode or restricted env; ensure no uncaught exceptions from storage calls. | Implement a storage util for safe access. |
| 7 | Persistence | `productSales` durability | `productSales` is in-memory only; if sales must be durable, persist it to `localStorage` or backend. | High | Buy a product, reload page, confirm sales counts persist if required. | Recommend serializing map to object in `saveToStorage()`. |
| 8 | State Mgmt | BehaviorSubject exposure | Verify components subscribe to Observables only (use `.asObservable()`), not to `.value` directly unless intentional. | Medium | Scan codebase for `.value` usages in components. | Prefer immutable updates via new arrays/objects. |
| 9 | IDs & Concurrency | `nextId` generation | Check for potential ID collisions in multi-tab or multiple clients; consider UUIDs or persisted counters. | Medium | Open two tabs and add products/users to see ID behavior. | Note: OK for local demo only. |
| 10 | Business Logic | `purchaseProduct` validation | Ensure `purchaseProduct` enforces login, `role === 'buyer'` and sufficient stock; check clear error messages returned. | High | Attempt purchase as logged-out, as seller, and with insufficient quantity; assert returned errors. | Unit tests recommended. |
| 11 | Data | products init heuristic | Review `needsUpdate` logic that may overwrite user-modified products; ensure intended behavior and document it. | High | Modify local products to fewer than 8; reload and confirm whether overwritten. | Consider a migration strategy instead of silent overwrite. |
| 12 | UI & UX | Login `returnUrl` behavior | Login should redirect to `returnUrl` (if present) rather than default `/dashboard`. | Medium | Simulate guard redirect with `?returnUrl=/payment/1/1` and complete login; expect redirect there. | Update `LoginComponent` to read router query params. |
| 13 | Accessibility | Forms labels & ARIA | Ensure form fields have labels and ARIA roles, popup dialogs manage focus. | Medium | Run lighthouse/accessibility checks on login and key pages. | Add `aria-live` for error messages. |
| 14 | Testing | Unit tests for `UserService` | Add unit tests covering login/logout, `addUser` (duplicate), `addProduct`, `purchaseProduct` success/failure. | High | Run `ng test` and confirm new tests pass. | Include edge cases and storage mocking. |
| 15 | CI & Lint | Enforce lint and tests in CI | Ensure pipeline runs lint, typecheck and unit tests; PRs fail if checks fail. | High | Open PR with intentional lint error to verify CI rejects it (test env permitting). | Update pipeline config if missing. |
| 16 | Logging | PII in logs/localStorage | Avoid logging sensitive user data to console or storing unnecessary PII in `localStorage`. | High | Scan code for `console.log` calls that include user objects and sanitize them. | Replace verbose logs with debug-level guarded logs. |
| 17 | Multi-tab | Logout sync across tabs | Consider using storage events to sync logout/login across browser tabs if needed. | Low | Login in one tab and logout in another; observe state handling. | Optional enhancement. |
| 18 | Docs | README persistence keys | Document which `localStorage` keys are used (`currentUser`, `users`, `products`, `seller_{id}_products`) and how to reset. | Low | Open README and confirm doc presence. | Add migration steps if structure changes. |

---

## Quick PR template (copy into PR description)

- Summary of change
- Checklist:
  - [ ] Tests added/updated
  - [ ] Lint and typecheck passed
  - [ ] Covered security concerns (if any)
  - [ ] LocalStorage handling reviewed (if changed)
  - [ ] Accessibility checks added/verified (if UI changes)
- Manual test steps

---

## Recommended immediate fixes (priority order)
1. Wrap all `localStorage` calls in a safe wrapper (try/catch + fallback in-memory store). (Critical)
2. Persist `productSales` to `localStorage` to make sales durable. (High)
3. Update `LoginComponent` to honor `returnUrl` query parameter from `authGuard`. (High)
4. Add unit tests for `UserService.purchaseProduct` and `addUser` duplicate flow. (High)

---

If you'd like, I can implement any of the recommended fixes or add unit tests now. Tell me which one to start with.