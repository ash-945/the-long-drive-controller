"""Package a verified plugin with player-facing Nexus release documents only."""
import hashlib
import json
from pathlib import Path
import re
import zipfile
from project import VERSION

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "release"


def main():
    source = ROOT / "dist" / ("ControllerPrototype-" + VERSION)
    build = json.loads((source / "build-info.json").read_text())
    plugin_name = "BepInEx/plugins/LongDrive.Controller/LongDrive.Controller.dll"
    plugin = (source / plugin_name).read_bytes()
    if build["version"] != VERSION or hashlib.sha256(plugin).hexdigest() != build["plugin_sha256"]:
        raise SystemExit("Build manifest does not match the plugin; rebuild first.")
    if plugin[:2] != b"MZ":
        raise SystemExit("Expected a managed Windows plugin DLL.")

    # Keep Nexus-specific metadata outside the player download. The description
    # is available in plain Markdown and BBCode for the listing editor.
    description = (DOCS / "NEXUS_DESCRIPTION.md").read_text()
    bbcode = re.sub(r"\[([^\]]+)\]\((https?://[^)]+)\)", r"[url=\2]\1[/url]", description)
    bbcode = re.sub(r"^### (.+)$", r"[b]\1[/b]", bbcode, flags=re.M)
    bbcode = re.sub(r"^## (.+)$", r"[size=4][b]\1[/b][/size]", bbcode, flags=re.M)
    bbcode = re.sub(r"^# (.+)$", r"[size=5][b]\1[/b][/size]", bbcode, flags=re.M)
    bbcode = re.sub(r"\*\*(.+?)\*\*", r"[b]\1[/b]", bbcode)
    bbcode = re.sub(r"`([^`]+)`", r"\1", bbcode)
    bbcode = re.sub(r"^- ", "• ", bbcode, flags=re.M)
    (DOCS / "NEXUS_DESCRIPTION_BBCODE.txt").write_text(bbcode)

    files = {plugin_name: plugin, "LICENSE": (ROOT / "LICENSE").read_bytes()}
    for name in ("INSTALL.txt", "CONTROLS.md", "CHANGELOG.txt", "CREDITS.txt"):
        files[name] = (DOCS / name).read_bytes()
    manifest = dict(build, release_channel="beta", installation="manual-game-root",
                    files={name: hashlib.sha256(data).hexdigest() for name, data in files.items()})
    files["release-manifest.json"] = (json.dumps(manifest, indent=2) + "\n").encode()
    for name, data in files.items():
        if name.startswith("/") or ".." in Path(name).parts:
            raise SystemExit("Unsafe archive path: " + name)
        if name.endswith((".txt", ".md", ".json")) and b"/home/ashley" in data:
            raise SystemExit("Private workspace path in public document: " + name)

    output = ROOT / "dist/nexus"
    output.mkdir(parents=True, exist_ok=True)
    archive = output / ("LongDrive-XInput-Controller-Support-" + VERSION + "-Beta.zip")
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as z:
        for name, data in files.items():
            z.writestr(name, data)
    with zipfile.ZipFile(archive) as z:
        if z.testzip() is not None or set(z.namelist()) != set(files):
            raise SystemExit("Archive verification failed.")
        packaged = json.loads(z.read("release-manifest.json"))
        for name, expected in packaged["files"].items():
            if hashlib.sha256(z.read(name)).hexdigest() != expected:
                raise SystemExit("Packaged file hash mismatch: " + name)
    checksum = hashlib.sha256(archive.read_bytes()).hexdigest()
    archive.with_suffix(".zip.sha256").write_text(checksum + "  " + archive.name + "\n")
    print("Nexus package:", archive)
    print("Verified", len(files), "archive entries; plugin matches build manifest.")
    print("Description:", DOCS / "NEXUS_DESCRIPTION.md")


if __name__ == "__main__":
    main()
