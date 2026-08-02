# Publishing Inviter from this repo

This repo is set up so that pushing to `main` automatically builds the
plugin, publishes a GitHub Release with the plugin zip, and keeps
`repo.json` (the file Dalamud reads as a custom plugin repository)
pointing at that release. One repo, no external plugin-index repo needed.

## One-time setup

1. **Push this to your own GitHub repo** (fork `Bluefissure/Inviter` or
   create a new one and push this content to it).
2. **Replace the placeholders** in `repo.json` (`YOUR_GH_USER/YOUR_REPO`)
   with your actual `owner/repo`. This affects `RepoUrl`, `IconUrl`, and
   the three `DownloadLink*` fields.
3. Make sure **Actions → General → Workflow permissions** is set to
   "Read and write permissions" for the repo (Settings → Actions →
   General), so the workflow can push the `repo.json` update and create
   releases with the default `GITHUB_TOKEN`.
4. Push. The workflow at `.github/workflows/build.yml` will:
   - spin up a Windows runner (required — Dalamud plugins need the
     native ImGui/interop bits, which only build on Windows),
   - download the current Release-channel Dalamud build from
     `goatcorp/dalamud-distrib` to build against (this is the same
     build XIVLauncher installs to end users),
   - `dotnet build` the plugin in Release mode, which also runs
     DalamudPackager and produces `latest.zip`,
   - read the actual `AssemblyVersion` / `DalamudApiLevel` out of the
     packed manifest (so `repo.json` never gets out of sync with what
     was actually built),
   - create a GitHub Release tagged `v<version>` with `latest.zip`
     attached, marked as the "latest" release,
   - update and commit `repo.json` with the new version/API
     level/timestamp.

## Everyday workflow

From here on, bump `<Version>` in `Inviter/Inviter.csproj`, commit, push
to `main` — the release and `repo.json` update happen automatically.
You can also trigger a rebuild manually from the Actions tab
(`workflow_dispatch`) without changing the version, e.g. to pick up a
new Dalamud API level.

## Adding it to Dalamud

In-game or via `/xlsettings` → Experimental → Custom Plugin Repositories,
add:

```
https://raw.githubusercontent.com/YOUR_GH_USER/YOUR_REPO/main/repo.json
```

then `/xlplugins` → search "Inviter" → Install. Updates will show up the
same way any other plugin's updates do, since `repo.json`'s
`AssemblyVersion` is what Dalamud diffs against.

## Notes

- `DownloadLinkInstall`/`Update`/`Testing` all point at
  `.../releases/latest/download/latest.zip`, which GitHub resolves to
  whatever release is currently marked "latest" — so that URL never
  needs to change, only `repo.json`'s version/timestamp fields do
  (which the workflow handles for you).
- If you ever want a testing/beta channel, add `TestingAssemblyVersion`,
  `TestingDalamudApiLevel`, and `IsTestingExclusive` keys to the
  `repo.json` entry — see
  <https://dalamud.dev/plugin-publishing/custom-repositories/>.
