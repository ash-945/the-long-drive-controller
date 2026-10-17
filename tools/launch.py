"""Launch the isolated local prototype copy under an installed Proton."""
import argparse
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--proton", type=Path, default=Path.home() / ".local/share/Steam/steamapps/common/Proton - Experimental/proton")
    parser.add_argument("--game-dir", type=Path, default=ROOT / ".local/test-game")
    parser.add_argument("--seconds", type=int, help="Bounded graphical smoke test; otherwise stays open until quit")
    parser.add_argument("--capture", action="store_true", help="Capture the game window after its menu initializes")
    parser.add_argument("--self-test", action="store_true", help="Read opt-in virtual controllers in isolation; do not drive gameplay")
    parser.add_argument("--vanilla", action="store_true", help="Bypass the loader for a baseline comparison")
    args = parser.parse_args()
    game = args.game_dir.resolve()
    if not args.proton.is_file() or not (game / "TheLongDrive.exe").is_file():
        parser.error("Proton or local test game missing; supply --proton and/or --game-dir")
    prefix = ROOT / ".local/proton-smoke"
    prefix.mkdir(parents=True, exist_ok=True)
    log_path = prefix / ("unity-vanilla.log" if args.vanilla else "unity-windowed.log")
    log = "Z:" + str(log_path).replace("/", "\\")
    env = dict(os.environ, STEAM_COMPAT_CLIENT_INSTALL_PATH=str(Path.home() / ".local/share/Steam"),
               STEAM_COMPAT_DATA_PATH=str(prefix), WINEDLLOVERRIDES="winhttp=b" if args.vanilla else "winhttp=n,b", WINEDEBUG="-all")
    command = [str(args.proton), "run", str(game / "TheLongDrive.exe"), "-screen-fullscreen", "0", "-screen-width", "1280", "-screen-height", "720", "-logFile", log]
    if args.capture:
        command.append("--controller-capture")
    if args.self_test:
        command.append("--controller-self-test")
    if args.seconds:
        command = ["timeout", "--signal=TERM", "--kill-after=5s", str(args.seconds) + "s"] + command
    print("Starting isolated game copy; log:", log_path, flush=True)
    result = subprocess.call(command, cwd=game, env=env)
    # Killing Proton's waiting Python process does not necessarily kill Wine children.
    # Limit cleanup to this workspace's dedicated test prefix.
    if args.seconds and result in (124, 137):
        server = args.proton.parent / "files/bin/wineserver"
        if server.is_file():
            subprocess.run([str(server), "-k"], env=dict(env, WINEPREFIX=str(prefix / "pfx")), check=False)
            subprocess.run([str(server), "-w"], env=dict(env, WINEPREFIX=str(prefix / "pfx")), check=False, timeout=15)
    raise SystemExit(result)


if __name__ == "__main__":
    main()
