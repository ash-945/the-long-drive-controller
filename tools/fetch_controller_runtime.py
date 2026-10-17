"""Fetch the pinned official Windows x64 SDL2 runtime for Windows and Proton."""
import hashlib
from pathlib import Path
import urllib.request
import zipfile

ROOT = Path(__file__).resolve().parents[1]
VERSION = "2.32.10"
URL = f"https://www.libsdl.org/release/SDL2-{VERSION}-win32-x64.zip"
SHA256 = "6cf9706eefd0a4a06dc764007934d428afaf029fabdd408a9e646048c91e18fb"


def verify_runtime():
    archive = ROOT / ".tools/sdl2" / f"SDL2-{VERSION}-win32-x64.zip"
    runtime = ROOT / ".tools/sdl2/runtime/SDL2.dll"
    if not archive.is_file() or hashlib.sha256(archive.read_bytes()).hexdigest() != SHA256:
        raise SystemExit("Missing or modified SDL2 archive; run tools/fetch_controller_runtime.py")
    with zipfile.ZipFile(archive) as package:
        expected = package.read("SDL2.dll")
    if not runtime.is_file() or runtime.read_bytes() != expected:
        raise SystemExit("SDL2 runtime differs from the pinned archive; run tools/fetch_controller_runtime.py")
    pe = int.from_bytes(expected[60:64], "little")
    if expected[:2] != b"MZ" or expected[pe:pe+4] != b"PE\0\0" or expected[pe+4:pe+6] != b"\x64\x86":
        raise SystemExit("Expected Windows x64 SDL2 runtime")
    return runtime


def main():
    cache = ROOT / ".tools/sdl2"
    cache.mkdir(parents=True, exist_ok=True)
    archive = cache / f"SDL2-{VERSION}-win32-x64.zip"
    if not archive.exists():
        with urllib.request.urlopen(URL, timeout=60) as response:
            data = response.read()
        if hashlib.sha256(data).hexdigest() != SHA256:
            raise SystemExit("SDL2 download hash mismatch")
        archive.write_bytes(data)
    if hashlib.sha256(archive.read_bytes()).hexdigest() != SHA256:
        raise SystemExit("Cached SDL2 archive hash mismatch")
    destination = cache / "runtime"
    destination.mkdir(exist_ok=True)
    with zipfile.ZipFile(archive) as package:
        for name in ("SDL2.dll", "README-SDL.txt"):
            (destination / name).write_bytes(package.read(name))
    verify_runtime()
    print("Verified official SDL2", VERSION, "Windows x64 runtime:", destination)


if __name__ == "__main__":
    main()
