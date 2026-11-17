# Provenance and license boundary

> **Status 2.1.2 — backend dihapus.** Panel taskbar kini hanya memakai popup WPF. Source
> `AwqatTaskbarRenderer.cs`, project `PrayerAutomation.Taskbar.WinUI.csproj`, dan
> `NoOptionalHelperExecutables.targets` sudah dihapus dari repository. File lisensi/NOTICE di
> direktori ini **sengaja dipertahankan** sebagai arsip atribusi dan syarat redistribusi untuk
> build 2.1.0/2.1.1 yang pernah didistribusikan. Jangan hapus tanpa meninjau kewajiban lisensi.

## Adapted Awqat source
- Repository: https://github.com/Khiro95/Awqat-Salaat
- Exact upstream pin: `c77a733efad9dc986901f07236e886b0bd6642ff`.
- Pipeline reference: https://github.com/Khiro95/Awqat-Salaat/blob/c77a733efad9dc986901f07236e886b0bd6642ff/AwqatSalaat.WinUI/TaskBarWidget.cs#L82-L115
- Native class/create helpers: same file lines 569-600.
- P/Invoke declaration subset: `AwqatSalaat.Common/Interop/Native.cs`, lines 7,38-48,56-75,80,103-104,142-143,182-198.
- `AwqatTaskbarRenderer.cs` is an adaptation, NOT a byte-identical upstream class. Retains the real DesktopWindowXamlSource → CreateWindowEx popup/layered → WindowId/AppWindow → ResizeClient → Initialize/SiteBridge → code-built Grid content → SetParent → owned AppWindow bounds/show pipeline. Local host-loss cleanup closes the island without setting Content after WM_NCDESTROY, because that setter dereferences the already-destroyed SiteBridge.
- Changes: noactivation/toolwindow policy, unique owned class and rooted delegate, errors/readbacks, no blocking retry, partial cleanup, no shell discovery or location/schedule/settings/audio logic.
- Include full `LICENSE-Awqat-Salaat.txt`: MIT, Copyright (c) 2023 Khiro.
- NO CSDeskBand, H.NotifyIcon, WindowsDisplayAPI, or StackOverflow-origin child enumerator source is copied.

## SDK bootstrap adaptation
- Stable version verified from actual NuGet index, downloaded package nuspec and GitHub latest release: `Microsoft.WindowsAppSDK.WinUI 2.6.4`; GitHub `v2.6.4`, published `2026-10-27T21:14:41Z` (evidence `latest-release.json`).
- WinUI component package only, not the larger AI/ML/Widgets aggregate WindowsAppSDK package.
- Explicit app-local bootstrap matches package `Microsoft.WindowsAppSDK.Foundation 2.6.4/include/UndockedRegFreeWinRT-AutoInitializer.cs`: process-local base-directory + PID variables and `WindowsAppRuntime_EnsureIsLoaded` export. No MSIX bootstrap, deployment-manager call or install.
- Bootstrap source's Microsoft MIT notice is retained in the backend header; full upstream `v2.6.4/LICENSE` copied to `LICENSE-WindowsAppSDK-Source.txt`.
- Runtime binary distribution is NOT automatically MIT-only: actual NuGet `license.txt` is Microsoft Software License Terms. Full copies for the four distributed SDK component packages are in `THIRD_PARTY_LICENSES/`; review especially distribution requirements/restrictions and retain applicable third-party notices.
- WebView2 is a transitive WinUI dependency, not instantiated by this backend. Preserve its package terms when redistributing its loader/projection; no WebView2 runtime installer is invoked.
- Existing .NET/Windows desktop runtime redistribution notices remain the final application's packaging responsibility.
- Upstream Awqat's 1.5.240311000/.NET6 is old; this deliverable does NOT ship or claim support for that SDK. It uses tested .NET10 + current 2.6.4. This is not an upstream 1.5 binary-fidelity test.
