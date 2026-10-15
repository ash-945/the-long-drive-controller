# Working on the controller mod

This repository holds the mod source, embedded controller graphics, build tools,
core checks, and release documentation. Game installations, decompiled game
source, loader binaries, local SDKs, saves, and generated packages stay outside
version control.

## Build and verify

Use Python 3, .NET SDK 8, and a local installation of The Long Drive
v2024.11.26b. Extract the official BepInEx 5.4.23.5 Windows x64 distribution into
`.tools/bepinex`. Both Windows and Proton use the Windows loader and plugin.

```sh
python3 tools/build.py "/path/to/The Long Drive"
```

The build compiles against your local game assemblies, runs the core checks,
and creates the plugin archive in `dist/`. Game assemblies are never committed
or packaged. Core checks verify action metadata and input behavior; game UI and
vehicle interactions also need hands-on testing.

## Develop and release

Work on a branch, make focused changes, and run the build before merging.
Keep the plugin version, project version, and `tools/project.py` version in
sync when preparing a release. Update player-facing documentation in `release/`
and generate the Nexus package after a successful build:

```sh
python3 tools/package_nexus.py
```

Install into a separate game copy for testing. Do not commit credentials,
personal configuration, or game files. Preserve existing user settings when
changing configuration or installation behavior.
