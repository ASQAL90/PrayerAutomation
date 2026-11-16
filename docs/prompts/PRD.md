# PRD — Ayo Jama'ah! (Rust Core + WPF)

**Branding 2.1.0:** display `Ayo Jama'ah!`, executable `AyoJamaah.exe`, approved logo master v13. Legacy storage/mutex/startup/installer identities and source namespaces remain for upgrade compatibility; dated acceptance below does not cover the new release.

**Status:** UI/UX accepted by user on 2026-10-30; per-user/no-admin C#+Rust installer authorized. Original design below remains historical context.  
**Target:** Windows 11 x64; desktop app; Indonesian UI by default.  
**Architecture selected:** Rust domain core + native C# WPF interface.  
**Deployment:** portable folder; no installer unless explicitly requested.  
**Existing app:** Python source/helper/mockups are archived outside the Git working tree with SHA256 receipts under `../PrayerAutomation-archives/2026-10-30-cleanup/legacy-python/`. Accepted C#+Rust portable is preserved under `../PrayerAutomation-releases/2.0.0/portable/`. Running/startup-referenced legacy paths remain protected until the user transitions normally.

## 1. Problem
The current settings screen has controls clipped by its form, the annual calendar can lag or close unexpectedly on first open, and a tall gray hover tooltip can obscure date cells. The owner wants a UI that is easier to arrange and customize and has proposed moving domain logic to Rust.

These are observed usability reports, not a proven CPU bottleneck. UI layout, overlays and event lifecycle require GUI-level fixes and measurement; adopting Rust alone does not resolve them or guarantee a whole-app performance improvement.

## 2. Goals
1. Provide a maintainable Windows UI with responsive forms, readable/selectable calendar cells, no blocking overlay tooltip, and observable errors.
2. Preserve the existing supported behavior and user configuration during a full eventual rewrite.
3. Keep calculation/calendar domain behavior deterministic, testable, and independently callable in a Rust core.
4. Deliver a portable build with its required runtime and native dependencies; provide a rollback path until acceptance.
5. Establish reproducible latency measurements and evidence for any performance claim.

## 3. Out of scope
- Claiming Rust automatically improves total CPU/RAM, or optimizing without a measured baseline.
- Changing prayer calculation standards, local correction values, country/city policy, or timezone semantics without an explicit product decision and evidence.
- Automatic geolocation from a specific provider as a required feature; preserve the current behavior after inventory.
- Altering desktop wallpaper, elevating the main app, or omitting lock-screen state restoration.
- Publishing/installer generation, silent fleet-wide deployment, or removing the Python fallback before explicit acceptance.

## 4. Users and main workflows
The primary user operates this Windows app through normal UI/UX use rather than source code. They need to review/configure a location and prayer standard, see daily schedules/reminders, browse an annual Gregorian/Hijri calendar, select dates to inspect details, and control startup/reminder/lock behavior.

## 5. Functional requirements
- **Settings:** in this order: (1) location name, latitude, longitude, timezone and calculation standard; (2) manual minute corrections; (3) GPS/network location action; (4) calendar date corrections; (5) reminder, workstation lock and Windows-login startup controls; (6) Save button. Preserve any current “today’s schedule” display and its placement after inventory.
- **Layout:** controls remain reachable by scrolling/resizing; no fixed window height that clips fields. Labels, focus, validation errors and keyboard navigation are clear.
- **Calendar:** annual Gregorian and Hijri views, correct 12 Hijri months and configured weekday order. Each calendar has a year selection box/dropdown, defaulting to the current year in that calendar system; a Query/Load action loads the selected year. Support past and future year options in an explicitly defined range, keep displayed year and selected year in sync after month navigation, and validate year boundaries. Month navigation, corrections and existing date information remain available. Date cells are interactive; show selected-date detail in a stable panel, not a hover window covering calendar cells. No hover tooltip is required for core information.
- **Prayer schedule:** preserve the current supported calculation methods, time zone behavior, manual offsets, derived times and offline behavior as confirmed by source/fixtures. Never infer a universal correction from a single date.
- **Reminders:** preserve user-configurable timing/message and silent text-only popup preference. Prevent duplicate firing and honor enable/disable.
- **Location:** retain available manual/GPS/network options as implemented today. Do not hardcode country, city, ISP or corporate-network denial rules.
- **Windows integration:** maintain existing tray/widget/startup/lock features only after inventory. Keep startup optional and user-controlled. Keep lock-screen behavior separate from desktop wallpaper and preserve snapshot/restore and no-elevation boundary.
- **Configuration:** identify existing schema and data path before migration; validate inputs and migrate without losing user values; retain a backup/rollback path. Do not commit personal config/secrets.
- **Diagnostics:** actionable, redacted error logs; avoid location/identity/secrets unless essential and explicitly consented.

## 6. Architecture
- `src/src/rust-core/`: pure domain library with Rust unit/integration tests; no GUI or Windows side effects in domain functions. Export a narrow, versioned C ABI (`cdylib`) only where used by the WPF host.
- `src/src/PrayerAutomation.UI/`: C# WPF application. Use responsive Grid/ScrollViewer layout, MVVM-style binding where appropriate, async work off the UI dispatcher and cancellation for long operations. Calendar cells remain selectable and detail stays in-page.
- FFI contract: document ABI version, integer/error codes, DTO layout, UTF-8 ownership, memory lifetime/free functions and panic containment before exposing complex data. No Rust-owned allocation crosses boundary without an explicit release contract.
- Windows-specific APIs belong in a small, testable adapter layer. Prevent old and new apps from simultaneously owning tray identifiers, startup entries, reminders or lock state.
- Keep the Python application untouched as a rollback/reference during development.

## 7. Nonfunctional requirements / acceptance gates
- **Settings usability:** every required field/control reachable without clipping at tested minimum resolution and 100%/150% display scaling; tab order and keyboard access usable. Record target resolutions before final acceptance.
- **Year selection / Query (Gregorian and Hijri):** each calendar opens with its current year selected; choosing a year and activating Query displays that year; past/future options are available inside the documented range; month navigation does not leave the selection display stale; invalid/out-of-range year selection is rejected safely.
- **Calendar interaction:** first open from a fresh process succeeds; no modal/unhandled crash; all date cells are legible and selectable with mouse and keyboard; no overlay obscures cells. Collect first-open/render timing on target PC and compare with the old build before setting a numeric performance commitment.
- **Correctness:** deterministic fixtures for schedule methods, offsets and time-zone boundaries; calendar conversion/navigation/correction edge cases; config validation and migration. Cross-check with existing implementation/reference data, treating reference schedule provenance explicitly.
- **Reliability:** no unhandled exception for invalid settings; failures do not lose config or leave the lock state unrestored; shutdown/restart doesn't schedule duplicate reminders.
- **Performance:** baseline and replacement are measured using equivalent machine, inputs, fresh/repeated launch state and documented timing method. Do not use compiler success as performance proof.
- **Portable release:** clean-folder launch with no dev tools installed, required .NET runtime/dependencies explicitly bundled or documented, correct Rust native DLL architecture, and preserved user config migration. Installer is excluded absent request.
- **Cutover:** feature parity checklist reviewed; Rust tests and UI/build checks pass; user accepts normal interaction. Until then, Python remains available.

## 8. Risks and mitigations
- **Feature loss in rewrite:** source/test-based parity inventory and one-slice-at-a-time implementation; keep old build.
- **FFI ownership/ABI errors:** stable minimal ABI, explicit ownership and boundary tests; start with scalar validations.
- **Calendar wrong but visually fast:** test date semantics separately from visual responsiveness against existing known fixtures.
- **WPF runtime packaging mismatch:** validate self-contained/portable publish on clean Windows environment before cutover.
- **Location/timezone variation:** avoid location-specific hardcoding; test manual coordinates, timezone and DST boundary behavior.
- **Lock-screen adverse effects:** platform adapter with snapshot/restore tests, no elevation or wallpaper calls; don't enable new side effects without UI control and tests.
- **UI accepted only by build:** require visual interaction and owner acceptance; screenshots/automation supplement but do not replace.

## 9. Rollout and definition of done
1. Inventory present features, settings schema, side effects, tests, dependencies and current baseline; record uncertainties.
2. Establish Rust core and WPF shell; prove ABI path with a small deterministic validation vertical slice.
3. Port calendar and responsive settings; fix observable overlay, clipping and first-open issue; measure.
4. Port schedule/config and then Windows integrations with regression tests.
5. Publish portable output; fresh-folder launch and configuration-preservation tests; report exact build artifact.
6. User tries normal UI/UX and confirms acceptance. Only then consider retiring the Python path; installer requires separate explicit request.

**Definition of done:** every inventoried in-scope capability implemented or explicitly deferred by user; correctness/regression checks pass; portable artifact launches cleanly; settings/calendar acceptance evidence is recorded; no secret/config included; no unapproved installer or destructive cutover; unresolved limits are disclosed.

## 10. Initial development slice
The first slice is limited to (a) this PRD, (b) Rust core ABI/version and deterministic coordinate/manual-correction validation with unit tests, and (c) a compilable WPF shell. It is scaffolding, not a complete app and does not claim to fix calendar/settings defects until those UI workflows are implemented and exercised.
