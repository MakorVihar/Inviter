# Inviter

A [Dalamud](https://dalamud.dev) plugin that automatically invites players who say a matching phrase (default "inv") in chat to your party.

This is a fork of [Bluefissure/Inviter](https://github.com/Bluefissure/Inviter), updated to target the current Dalamud API and extended with a server info bar toggle, a right-click quick menu, channel filter shortcuts, and per-zone rules.

![Settings window](images/settings_window.jpg)

## Features

- Automatically invites players whose chat message matches a text pattern or regex you configure
- Per-channel filters, so you can pick exactly which chat channels are checked, with "All" / "Clear" shortcuts
- Per-zone rules: override the pattern (and whether it's regex) for specific zones, or force auto-invite on/off in specific zones regardless of the main switch
- A server info bar (DTR) entry showing the effective status for whatever zone you're currently in:
  - `On` / `Off` — following the main switch, no zone rule for this zone
  - `Inherited - On` / `Inherited - Off` — this zone has a rule, but it's set to Inherit, so it's still following the main switch
  - `Forced - On` / `Forced - Off` — this zone has a Force On/Off rule, overriding the main switch
  - `Timed (Xm)` — a timed session is running and currently in effect
  - **Left-click** the entry to toggle. In a Forced zone this flips the force state directly (Forced - On ↔ Forced - Off) without touching the main switch; everywhere else it toggles the main switch
  - **Right-click** the entry for a quick menu: turn on/off, start a timed session (with duration presets or a custom minutes/attempts input), cancel a running timed session, or open settings

![Server info bar entry](images/server_info_bar_entry.jpg)

![Right-click menu](images/right_click_menu.jpg)

## Commands

| Command | Effect |
|---|---|
| `/xinvite` | Open the settings window |
| `/xinvite on` / `/xinvite off` / `/xinvite toggle` | Turn auto-invite on/off/toggle it |
| `/xinvite <minutes>` | Enable auto-invite for a set number of minutes |
| `/xinvite <minutes> <attempts>` | Enable auto-invite for a set number of minutes, ending early after that many invite attempts |
| `/xinvite 0` | Cancel a running timed session |

The right-click menu and most left-clicks are just a UI for these same commands. The one exception is left-clicking the DTR entry while in a Forced zone, which flips that zone's force state directly rather than running a command — see the DTR bullet above.

## Per-zone rules

The settings window's "...but for these Maps" table lets you override behavior for specific zones. Each row has:

| Column | Effect |
|---|---|
| Mode | `Inherit` (follow the main Enable switch), `Force On` (run here even if the main switch is off), or `Force Off` (never run here, even if the main switch is on) |
| Pattern override | Check the box to give this zone its own text pattern instead of the global one |
| Regex | Only enabled when this zone has a pattern override — whether *that* pattern is regex. There's no separate "inherit" for this: a zone either uses its own pattern (and decides for itself whether it's regex) or uses the global pattern (and the global regex setting) |
| Remove | Deletes the rule for that zone |

Use **Add Current Zone** to add a rule for wherever you're currently standing, or type into **Search zone name...** to add any zone by name.

## Example setup

A worked example, walking through every part of the config using one scenario: **auto-inviting for hunt trains and using a stricter pattern in a North Horn (Occult Crescent) where "lfg" gets used for two different things.**

### 1. General Settings

- **Enable** — set this **on** in our example.
- **Tooltips** — on, just for convenience while configuring.
- **Server Info Bar** — on, so the DTR entry shows current status (see section 4).
- **Pattern** — (empty), **Regex** off. This is the global default: anywhere without a zone-specific pattern override falls back to this. An empty pattern will not match anything.
- **Delay (ms)** — a randomized wait between the two values you set here, before actually sending the invite; e.g. `200` to `600` means each invite waits somewhere between 200ms and 600ms, not always the same amount.
- **Rate limit (ms)** — how long to wait between invites, so as to not trigger a burst of invites.

<!-- ![General settings](images/example-general-settings.png) -->

### 2. Filters

Pick which chat channels are actually watched. For a hunt train you mostly care about **Shout** and **Yell** — leave channels like Free Company unchecked if you don't want messages from there triggering an invite too.

<!-- ![Channel filters](images/example-filters.png) -->

### 3. Per-zone rules ("...but for these Maps")

Three example rows, each demonstrating a different column:

| Mode | Zone | Pattern override | Regex | Why |
|---|---|---|---|---|
| `Force On` | (the hunt train zone) | `inv` | — | Auto-invite runs here regardless of whether the main switch above is on or off |
| `Inherit` | North Horn | `^(?!.*tower).*lfg.*$` | on | Follows the main switch like normal, but uses this zone's own pattern instead of the global `inv` — here, "lfg" is used for two groups in that zone, and this excludes one of them. |

Add a row with **Add Current Zone** while standing in the relevant zone, or find one elsewhere with **Search zone name...**.

<!-- ![Zone rules](images/example-zone-rules.png) -->

### 4. Server info bar, in this scenario

With the setup above, walking around shows different DTR text depending on where you are:

- Most zones (no rule, main switch off): **`Inviter: Off`**
- The hunt train zone (`Force On`): **`Inviter: Forced - On`** — left-clicking here toggles it to `Forced - Off` without touching the main switch
- North Horn (`Inherit` + pattern override): **`Inviter: Inherited - On`**, same as the main switch since Inherit still follows this — left-clicking here toggles it to **`Inviter: Inherited - Off`**, turning off the main switch.
- If you right-click anywhere and start a timed session, that zone shows **`Inviter: Timed (Xm)`** instead, counting down

<!-- ![DTR bar states](images/example-dtr-states.png) -->

### Reference: pattern matching

| Pattern | Regex | Matches | Doesn't match |
|---|---|---|---|
| `inv` | off | any message containing "inv" — "inv", "invite", but also "invalid", "convince" | messages with no "inv" substring at all |
| `\binv\b` | on | "inv" as a standalone word | "invalid", "convince", "investment" — false positives from the plain substring version above |
| `\b(inv\|lfg)\b` | on | "inv" or "lfg" as standalone words | "invalid", "lfgroup" |
| `^(?!.*tower).*lfg.*$` | on | any message containing "lfg", as long as it doesn't also contain "tower" | "lfg ce", but not "lfg tower" |

## Install

Add this repository's `repo.json` as a custom plugin repository in Dalamud: **Dalamud Settings → Experimental → Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/MakorVihar/Inviter/main/repo.json
```

Then find "Inviter" under **Plugin Installer → All Plugins** and install it. See [PUBLISHING.md](PUBLISHING.md) if you're setting this repository up yourself.

## Credits

Originally created by [Bluefissure](https://github.com/Bluefissure/Inviter).
