"""Install into an explicitly selected game directory, refusing loader collisions."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
from project import VERSION

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("game_dir", type=Path)
    parser.add_argument("--with-loader", action="store_true", help="Install the downloaded BepInEx loader; refuses conflicting existing files")
    args = parser.parse_args()
    game = args.game_dir.resolve()
    assembly = game / "TheLongDrive_Data/Managed/Assembly-CSharp.dll"
    if not (game / "TheLongDrive.exe").is_file() or not assembly.is_file():
        parser.error("Not a The Long Drive managed Windows game folder")
    stage = ROOT / ("dist/ControllerPrototype-" + VERSION)
    manifest = json.loads((stage / "build-info.json").read_text())
    if hashlib.sha256(assembly.read_bytes()).hexdigest() != manifest["reference_assembly_sha256"]:
        parser.error("Game assembly differs from the build references. Reinspect/rebuild for this installation.")
    if not args.with_loader and not (game / "BepInEx/core/BepInEx.dll").is_file():
        parser.error("BepInEx not installed. Download the official Windows x64 5.4.23.5 loader, or use --with-loader after staging it in .tools/bepinex.")
    copies = []
    if args.with_loader:
        loader = ROOT / ".tools/bepinex"
        if not (loader / "BepInEx/core/BepInEx.dll").is_file():
            parser.error("Official loader missing from .tools/bepinex")
        for path in loader.rglob("*"):
            if path.is_file():
                target = game / path.relative_to(loader)
                if target.exists() and target.read_bytes() != path.read_bytes():
                    parser.error(f"Refusing to replace existing loader file: {target}")
                copies.append((path, target))
    source = stage / "BepInEx/plugins/LongDrive.Controller/LongDrive.Controller.dll"
    target = game / "BepInEx/plugins/LongDrive.Controller/LongDrive.Controller.dll"
    if target.exists() and target.read_bytes() != source.read_bytes():
        backup = target.with_name(target.name + ".previous-" + hashlib.sha256(target.read_bytes()).hexdigest()[:12])
        if backup.exists() and backup.read_bytes() != target.read_bytes():
            parser.error(f"Plugin backup hash collision: {backup}")
        if not backup.exists():
            shutil.copy2(target, backup)
    copies.append((source, target))
    for source, target in copies:
        target.parent.mkdir(parents=True, exist_ok=True)
        # Replace the file atomically so a running game's mapped plugin remains
        # intact. The updated assembly is picked up on the next game launch.
        pending = target.with_name(target.name + ".installing")
        shutil.copy2(source, pending)
        pending.replace(target)
    # This build destroys the visible BepInEx manager before Start/Update.
    # Preserve existing loader settings while enabling its supported compatibility option.
    config = game / "BepInEx/config/BepInEx.cfg"
    config.parent.mkdir(parents=True, exist_ok=True)
    content = config.read_text() if config.exists() else ""
    import re
    if re.search(r"(?m)^HideManagerGameObject\s*=", content):
        updated = re.sub(r"(?m)^HideManagerGameObject\s*=.*$", "HideManagerGameObject = true", content)
    elif "[Chainloader]" in content:
        updated = content.replace("[Chainloader]", "[Chainloader]\nHideManagerGameObject = true", 1)
    else:
        updated = content + "\n[Chainloader]\nHideManagerGameObject = true\n"
    if updated != content:
        if config.exists():
            backup = config.with_name(config.name + ".before-controller")
            if not backup.exists():
                shutil.copy2(config, backup)
        config.write_text(updated)
    print(f"Installed prototype into {game}. Original game assemblies were not modified.")


if __name__ == "__main__":
    main()
