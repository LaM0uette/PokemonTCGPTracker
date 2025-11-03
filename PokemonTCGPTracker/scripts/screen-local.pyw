#!/usr/bin/env python3
import ctypes
try:
    ctypes.windll.user32.SetProcessDPIAware()
except Exception:
    pass

import mss
from PIL import Image
import time
import os
import sys
import shutil
import requests
import mimetypes
import traceback

# === CONFIG SERVEUR ===
SERVER_BASE = "https://localhost:7298"
VERIFY_TLS = False
TIMEOUT = 30

# === MODE DEBUG ===
DEBUG_SAVE = True
DEBUG_DIR = os.path.join(os.path.expanduser("~"), "Documents", "PokemonTCGPTracker", "debug")

# === JOURNALISATION ===
LOG_FILE = os.path.join(DEBUG_DIR, "screen_log.txt")


def log(msg: str):
    """Écrit dans un fichier de log pour traçage."""
    os.makedirs(DEBUG_DIR, exist_ok=True)
    timestamp = time.strftime("[%Y-%m-%d %H:%M:%S]")
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(f"{timestamp} {msg}\n")
    print(msg)


def get_base_path():
    if getattr(sys, 'frozen', False):
        return os.path.dirname(sys.executable)
    return os.path.dirname(os.path.abspath(__file__))


def capture_fullscreen() -> str:
    """Capture l'écran principal et enregistre un fichier PNG."""
    base_path = get_base_path()
    with mss.mss() as sct:
        monitor = sct.monitors[1]
        screenshot = sct.grab(monitor)
        img = Image.frombytes("RGB", (screenshot.width, screenshot.height), screenshot.rgb)
        timestamp = time.strftime("%Y%m%d_%H%M%S")
        filename = os.path.join(base_path, f"screen_{timestamp}.png")
        img.save(filename)
        time.sleep(0.2)  # ✅ temps de flush disque
        log(f"✅ Capture sauvegardée : {filename} ({screenshot.width}x{screenshot.height})")
        return filename


def guess_mime(path: str) -> str:
    mime, _ = mimetypes.guess_type(path)
    return mime or "image/png"


def upload_image(file_path: str) -> str:
    """Upload l'image complète au serveur /deck/upload."""
    url = SERVER_BASE.rstrip("/") + "/deck/upload"
    mime = guess_mime(file_path)
    log(f"📤 Tentative d'upload vers {url}")

    try:
        with open(file_path, "rb") as f:
            files = {"file": (os.path.basename(file_path), f, mime)}
            resp = requests.post(url, files=files, timeout=TIMEOUT, verify=VERIFY_TLS)
        resp.raise_for_status()
        data = resp.json()
        uploaded_url = data.get("url")
        if not uploaded_url:
            raise RuntimeError(f"Réponse inattendue du serveur : {data}")
        log(f"✅ Upload réussi : {uploaded_url}")
        return uploaded_url
    except Exception as e:
        log(f"❌ Erreur upload : {e}")
        log(traceback.format_exc())
        raise


def main():
    try:
        filename = capture_fullscreen()
        uploaded_url = upload_image(filename)
    except Exception as e:
        log(f"❌ Erreur principale : {e}")
    finally:
        try:
            if DEBUG_SAVE:
                os.makedirs(DEBUG_DIR, exist_ok=True)
                dest_path = os.path.join(DEBUG_DIR, os.path.basename(filename))
                shutil.copy2(filename, dest_path)
                log(f"💾 Copie debug dans : {dest_path}")
            if os.path.exists(filename):
                os.remove(filename)
                log(f"🧹 Fichier temporaire supprimé : {filename}")
        except Exception as e:
            log(f"⚠️ Nettoyage/Debug échoué : {e}")


if __name__ == "__main__":
    main()
