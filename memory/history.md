# Task History

## 2026-05-10 - Initial Web App Scaffold

- Date: 2026-05-10
- Task: Create the first version of a personal requirements-capture web app.
- Changes: Copied and adapted agent docs from VABELRoutes; created a .NET 10 solution with Domain, AppCore, Contracts, Infrastructure and API layers; added JSON local persistence; built a static web UI for projects, surveys, voice/text mentions and requirement drafts.
- Pending: Add tests and improve export options when the first real workflow is clear.
- Risks: Browser dictation depends on Web Speech API support, best tested in Chrome/Edge.

## 2026-05-22 - Architecture Cleanup and File Split

- Date: 2026-05-22
- Task: Refactor oversized backend/frontend files into smaller responsibilities aligned with Clean Architecture.
- Changes: Split `RequirementWorkspaceService` responsibilities into template factory, workspace normalizer, participant factory, transcript formatter and response mapper; rewired DI in the API; converted the web app bootstrap into ES modules under `wwwroot/js` and separated project, survey, requirement and template flows from the old monolithic `app.js`.
- Pending: Keep decomposing the survey module, which is still the largest frontend module after the first extraction pass.
- Risks: The browser build now depends on module loading from `index.html`, so any future inline script assumptions should be avoided.

## 2026-05-23 - Dark Mode and Working Guardrails

- Date: 2026-05-23
- Task: Document the refactor and add first-class dark mode support.
- Changes: Added `docs/agent/development-guardrails.md` with architecture and UI rules; updated `AGENTS.md` to require that guide for refactor/frontend work; enabled persistent theme switching from the UI and extended CSS tokens so the app works in both clear and dark themes.
- Pending: Split `surveys.js` further and verify all dense screens visually in both themes on mobile.
- Risks: New UI surfaces must use theme tokens instead of ad hoc colors or the dark theme will drift.

## 2026-05-23 - Interview Flow Refinements

- Date: 2026-05-23
- Task: Tighten the interview flow around speaker labels, default participants, save ergonomics and capture buffers.
- Changes: Normalized transcript/minute output from `Yo` to `Entrevistador`; created a default guest participant for new and normalized surveys; added save-on-enter behavior for the main capture/edit fields; updated the next-question flow to request interviewee response before advancing; introduced per-context capture buffers so text survives participant/question switches while sync finishes in the background.
- Pending: Do a visual pass in the browser on long interviews to confirm the new buffer behavior feels natural on mobile.
- Risks: Browser speech recognition still has platform quirks, so background sync is safer now but still constrained by Web Speech API timing.

## 2026-05-23 - Chronological Interview Turns

- Date: 2026-05-23
- Task: Switch survey capture from per-person accumulation to chronological turns per question.
- Changes: Reworked the survey frontend so active capture updates no longer re-render the whole guide while typing or dictating; moved transcript/minute formatting toward chronological turn output with seconds; recovered question labels from template metadata when old tags contain `undefined`; added per-question progress markers for `Preguntada` and `Respondida`; surfaced `Entrevistador` consistently in the UI while keeping `Yo` internally for compatibility.
- Pending: Validate the flow in a real interview on mobile, especially rapid speaker switching while Web Speech API is still flushing results.
- Risks: Legacy `minuteDraft` text can still preserve older wording until it is regenerated from current transcript output.

## 2026-05-23 - Documentation Consolidation

- Date: 2026-05-23
- Task: Document the current state of the system, interview workflow and working agreements after multiple iterations on surveys, transcript and minute generation.
- Changes: Added `docs/agent/current-system-state.md` with architecture, current capabilities, key rules and fragile areas; added `docs/product/interview-workflow.md` with expected interview behavior; linked both documents from `AGENTS.md` and reinforced the guardrails so future changes to voice, transcript, minute and UX read those documents first.
- Pending: Keep the two documents synchronized as the survey module is split further or the interview flow changes.
- Risks: If future work updates implementation without updating these docs, agents may reintroduce already-closed product regressions.

## 2026-05-23 - Mobile LAN Publishing Prep

- Date: 2026-05-23
- Task: Prepare the app to run from the local network so it can be used from a phone.
- Changes: Added a `mobile-http` launch profile that listens beyond `localhost`; created `scripts/run_mobile.ps1` to start the API on `0.0.0.0` and print the LAN URL; created `scripts/publish_mobile.ps1` to generate a release folder; documented the recommended phone workflow in `docs/deployment/mobile-lan.md`.
- Pending: Validate on a real phone and decide later whether the app also needs an Internet-facing deployment path.
- Risks: Mobile speech recognition still depends on browser support and may require HTTPS in some environments.

## 2026-05-23 - Session Login And Online Deployment Prep

- Date: 2026-05-23
- Task: Add a minimal login flow so the app can be exposed online more safely and prepare it for container hosting.
- Changes: Added cookie-based auth with `Auth__Username` and `Auth__Password` server configuration; protected workspace API endpoints while keeping auth status/login/logout endpoints public; added a frontend login gate and logout flow; added `Dockerfile` and `.dockerignore`; documented online deployment requirements including persistent storage for `Workspace__DataFile`.
- Pending: Validate the login flow in a real hosted environment with `https` and mounted persistent storage.
- Risks: The app still uses a single shared password and local-file JSON persistence, so Internet deployment must mount persistent disk and treat the password as a secret.

## 2026-05-23 - Production Auth Route Fix

- Date: 2026-05-23
- Task: Fix production behavior where authenticated UI loaded but workspace actions did not respond.
- Changes: Split public auth endpoints into their own `/api/auth` route group and protected only the workspace `/api` group; verified locally that auth status, login, default template seeding and project creation work with auth enabled.
- Pending: Redeploy Railway and verify the production buttons after the new commit is live.
- Risks: If Railway volume permissions are misconfigured, workspace saves can still fail independently of auth.

## 2026-05-23 - Default Template Pack

- Date: 2026-05-23
- Task: Restore useful starter templates in production instead of shipping only the base interview template.
- Changes: Expanded the default system template factory to seed four templates: base requirements discovery, operational process, existing system improvement and reporting/dashboard; made normalization add missing default templates without duplicating existing templates by name.
- Pending: Redeploy Railway and confirm the system template catalog shows the full starter pack.
- Risks: Existing production workspaces only receive the new templates when the workspace is loaded and normalization can save to the configured persistent volume.

## 2026-05-23 - Minute Draft Regeneration

- Date: 2026-05-23
- Task: Fix cases where the validation minute only showed the survey title.
- Changes: The survey UI now regenerates minute text when the current draft has saved transcript turns but no section structure; saving/finalizing a minute first syncs pending capture buffers so recently dictated text is included.
- Pending: Redeploy Railway and retest saving/finalizing a survey with transcript turns.
- Risks: If no turns were saved at all, the minute can only show the header and suggested sections until capture sync succeeds.

## 2026-05-23 - Mobile Speech Stabilization

- Date: 2026-05-23
- Task: Stop Android microphone reconnect loops in production.
- Changes: Mobile browsers now use manual speech sessions instead of continuous auto-restart; when Android/iOS ends recognition, the app syncs current text, pauses capture and asks the user to tap start again.
- Pending: Redeploy Railway and validate microphone behavior on Android Chrome.
- Risks: Mobile dictation now requires more taps, but avoids the repeated connect/disconnect loop.
