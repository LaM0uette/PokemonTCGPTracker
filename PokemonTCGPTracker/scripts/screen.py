#!/usr/bin/env python3
import ctypes
try:
    # ✅ Corrige le problème de scaling DPI sur les écrans 4K Windows
    ctypes.windll.user32.SetProcessDPIAware()
except Exception:
    pass

import mss
from PIL import Image
import time
import os
import requests
import mimetypes

# === CONFIG SERVEUR ===
SERVER_BASE = "https://localhost:7298"   # ton endpoint ASP.NET
VERIFY_TLS = False                        # True si ton certificat est valide
TIMEOUT = 30


def capture_fullscreen() -> str:
    """Capture tout l'écran principal et enregistre un fichier PNG."""
    with mss.mss() as sct:
        monitor = sct.monitors[1]  # écran principal
        screenshot = sct.grab(monitor)

        img = Image.frombytes("RGB", (screenshot.width, screenshot.height), screenshot.rgb)
        timestamp = time.strftime("%Y%m%d_%H%M%S")
        filename = f"screen_{timestamp}.png"
        img.save(filename)
        print(f"✅ Capture plein écran sauvegardée : {filename}")
        return filename


def guess_mime(path: str) -> str:
    mime, _ = mimetypes.guess_type(path)
    if mime:
        return mime
    ext = os.path.splitext(path)[1].lower()
    return {
        ".png": "image/png",
        ".jpg": "image/jpeg",
        ".jpeg": "image/jpeg",
        ".gif": "image/gif",
        ".webp": "image/webp",
    }.get(ext, "application/octet-stream")


def upload_image(file_path: str) -> str:
    """Upload l'image complète au serveur /deck/upload."""
    url = SERVER_BASE.rstrip("/") + "/deck/upload"
    mime = guess_mime(file_path)

    with open(file_path, "rb") as f:
        files = {"file": (os.path.basename(file_path), f, mime)}
        print(f"📤 Upload vers {url} ...")
        resp = requests.post(url, files=files, timeout=TIMEOUT, verify=VERIFY_TLS)

    resp.raise_for_status()
    data = resp.json()
    uploaded_url = data.get("url")
    if not uploaded_url:
        raise RuntimeError(f"Réponse inattendue du serveur : {data}")
    return uploaded_url


def main():
    filename = capture_fullscreen()
    try:
        uploaded_url = upload_image(filename)
        print(f"✅ Upload réussi : {uploaded_url}")
    except Exception as e:
        print(f"❌ Erreur upload : {e}")
    finally:
        if os.path.exists(filename):
            os.remove(filename)
            print(f"🧹 Fichier temporaire supprimé : {filename}")


if __name__ == "__main__":
    main()
