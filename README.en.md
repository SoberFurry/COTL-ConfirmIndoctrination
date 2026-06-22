# Sam ConfirmIndoctrination

A BepInEx plugin for **Cult of the Lamb** that prevents accidentally starting the indoctrination of a
new waiting follower. Before the name/form screen opens it shows "Accept this follower into the cult?".

- GUID: `com.sam.cultofthelamb.confirmindoctrination`
- Built against: Unity 2022.3.62f2, build 22885603
- Dependency: BepInEx 5.4.x (COTL pack). COTL_API not required.

## Install
```
<GameFolder>\BepInEx\plugins\SamMods\ConfirmIndoctrination\
```

## Behaviour
- Interacting with a **new waiting** recruit shows a confirmation popup, default focus on **Cancel**.
- Accept is disabled until the interact button is released, plus a short debounce (200 ms default).
- Cancel (Esc/Back/B): the follower keeps waiting, nothing changes.
- Accept: the vanilla indoctrination screen opens exactly once (one-shot bypass keyed by follower id).
- Re-indoctrination of existing cultists is never intercepted (we only hook
  `FollowerRecruit.OnInteract/OnSecondaryInteract`, not the shared `ShowIndoctrinationMenu`).

## Input
Mouse (works under Remote Play), keyboard (←/→, Enter/Space, Esc), gamepad (south=confirm, east=cancel).

## Config
`Mode` (default **BeforeScreen** = safe/recommended; `InsideScreen`/`Both`/`Disabled`),
`DefaultSelection` (Cancel), `InputDebounceMs` (200), `AllowCancelForTutorial` (false),
`AllowCancelForIntegrations` (false), `VerboseLogging` (false).

The in-screen cancel (`InsideScreen`/`Both`) is experimental, fail-open and OFF by default; it never
uses `RemoveAllListeners`. It could not be runtime-verified here, so `BeforeScreen` is the guaranteed-safe path.

## Uninstall
Run `uninstall_sammods.ps1` or delete `BepInEx\plugins\SamMods\ConfirmIndoctrination\`.
