# Inviter

A [Dalamud](https://dalamud.dev) plugin that automatically invites players who say a matching phrase (default "inv") in chat to your party.

This is a fork of [Bluefissure/Inviter](https://github.com/Bluefissure/Inviter), updated to target the current Dalamud API and extended with a server info bar toggle, a right-click quick menu, and channel filter shortcuts.

<!-- TODO: replace with a real screenshot of the settings window -->
![Settings window](images/screenshot-settings.png)

## Features

- Automatically invites players whose chat message matches a text pattern or regex you configure
- Per-channel filters, so you can pick exactly which chat channels are checked, with "All" / "Clear" shortcuts
- A server info bar (DTR) entry showing whether auto-invite is on, off, or running a timed session
  - **Left-click** the entry to toggle auto-invite on/off
  - **Right-click** the entry for a quick menu: turn on/off, start a timed session (with duration presets or a custom minutes/attempts input), cancel a running timed session, or open settings

<!-- TODO: replace with a real screenshot of the server info bar entry -->
![Server info bar entry](images/screenshot-dtr.png)

<!-- TODO: replace with a real screenshot of the right-click menu -->
![Right-click menu](images/screenshot-dtr-menu.png)

## Commands

| Command | Effect |
|---|---|
| `/xinvite` | Open the settings window |
| `/xinvite on` / `/xinvite off` / `/xinvite toggle` | Turn auto-invite on/off/toggle it |
| `/xinvite <minutes>` | Enable auto-invite for a set number of minutes |
| `/xinvite <minutes> <attempts>` | Enable auto-invite for a set number of minutes, ending early after that many invite attempts |
| `/xinvite 0` | Cancel a running timed session |

The server info bar entry and its right-click menu are just a UI for these same commands — nothing behaves differently depending on how you trigger it.

## Install

Add this repository's `repo.json` as a custom plugin repository in Dalamud: **Dalamud Settings → Experimental → Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/MakorVihar/Inviter/main/repo.json
```

Then find "Inviter" under **Plugin Installer → All Plugins** and install it. See [PUBLISHING.md](PUBLISHING.md) if you're setting this repository up yourself.

## Known bugs

~~The plugin won't auto disable itself after the party is full, due to the [null PartyList](https://github.com/goatcorp/Dalamud/blob/3bfd6740d5c5e529b6b0d2f1cbd17fcc3c628588/Dalamud/Game/ClientState/PartyList.cs#L32) in Dalamud.~~ (Fixed since [v1.0.1.0](https://github.com/Bluefissure/Inviter/releases/tag/v1.0.1.0))

## Credits

Originally created by [Bluefissure](https://github.com/Bluefissure/Inviter).
