"""Build and package only our plugin. Game and loader binaries are never packaged."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import zipfile
from project import VERSION

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("game_dir", type=Path)
    parser.add_argument("--package-only", action="store_true", help="Refresh documentation/package after an already verified build")
    args = parser.parse_args()
    game = args.game_dir.resolve()
    if not (game / "TheLongDrive_Data/Managed/Assembly-CSharp.dll").is_file():
        parser.error("game_dir must contain TheLongDrive_Data/Managed/Assembly-CSharp.dll")
    dotnet = shutil.which("dotnet") or str(ROOT / ".tools/dotnet/dotnet")
    env = dict(os.environ, DOTNET_CLI_HOME=str(ROOT / ".local/dotnet-home"), DOTNET_CLI_TELEMETRY_OPTOUT="1")
    if not args.package_only:
        subprocess.run([dotnet, "build", str(ROOT / "src/LongDrive.Controller"), "-c", "Release", f"-p:GameDir={game}"], env=env, check=True, cwd=ROOT)
        subprocess.run([dotnet, "run", "--project", str(ROOT / "tests/Controller.CoreChecks"), "-c", "Release", "--", str(game / "TheLongDrive_Data/Managed/Assembly-CSharp.dll"), str(ROOT / "docs/ACTIONS.md")], env=env, check=True, cwd=ROOT)
    stage = ROOT / ("dist/ControllerPrototype-" + VERSION)
    plugin = stage / "BepInEx/plugins/LongDrive.Controller/LongDrive.Controller.dll"
    plugin.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / "src/LongDrive.Controller/bin/Release/netstandard2.1/LongDrive.Controller.dll", plugin)
    shutil.copy2(ROOT / "README.md", stage / "README.md")
    shutil.copy2(ROOT / "LICENSE", stage / "LICENSE")
    (stage / "docs").mkdir(exist_ok=True)
    shutil.copy2(ROOT / "docs/TESTING.md", stage / "docs/TESTING.md")
    shutil.copy2(ROOT / "docs/COMPATIBILITY.md", stage / "docs/COMPATIBILITY.md")
    shutil.copy2(ROOT / "docs/ACTIONS.md", stage / "docs/ACTIONS.md")
    shutil.copy2(ROOT / "docs/BINDINGS.md", stage / "docs/BINDINGS.md")
    manifest = {
        "version": VERSION,
        "game_version": "2024.11.26b_test",
        "loader": "BepInEx 5.4.23.5 Windows x64 (separate download)",
        "plugin_sha256": hashlib.sha256(plugin.read_bytes()).hexdigest(),
        "reference_assembly_sha256": hashlib.sha256((game / "TheLongDrive_Data/Managed/Assembly-CSharp.dll").read_bytes()).hexdigest(),
    }
    (stage / "build-info.json").write_text(json.dumps(manifest, indent=2) + "\n")
    archive = Path(str(stage) + ".zip")
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as output:
        for path in [plugin, stage / "README.md", stage / "LICENSE", stage / "docs/TESTING.md", stage / "docs/COMPATIBILITY.md", stage / "docs/ACTIONS.md", stage / "docs/BINDINGS.md", stage / "build-info.json"]:
            output.write(path, path.relative_to(stage))
    print(f"Ready: {archive}")


if __name__ == "__main__":
    main()
