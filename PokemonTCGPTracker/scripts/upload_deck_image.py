#!/usr/bin/env python3
"""
Upload a deck image to the PokemonTCGPTracker server.

This script posts an image file to the `/deck/upload` endpoint. When the upload
succeeds, the server broadcasts a SignalR event (DeckImageUpdated) to all
connected clients, and any open `/deck` page will refresh the image
automatically.

Usage examples:
  - Basic (default local dev server in HTTPS):
      python scripts/upload_deck_image.py path/to/image.png

  - Specify a custom server base URL:
      python scripts/upload_deck_image.py path/to/image.png --server https://my-host.example

  - During local development, if your dev certificate isn't trusted:
      python scripts/upload_deck_image.py path/to/image.png --insecure

Exit codes:
  0  success
  1  invalid arguments or file not found
  2  HTTP error (non-2xx)
  3  network error or unexpected failure
"""
from __future__ import annotations

import argparse
import mimetypes
import os
import sys
from typing import Optional

import requests


def guess_mime(path: str) -> str:
    mime, _ = mimetypes.guess_type(path)
    # Fallback to a sensible default for images
    if mime is None:
        # Try to infer from extension for common image types
        ext = os.path.splitext(path)[1].lower()
        if ext in {'.png'}:
            return 'image/png'
        if ext in {'.jpg', '.jpeg'}:
            return 'image/jpeg'
        if ext in {'.gif'}:
            return 'image/gif'
        if ext in {'.webp'}:
            return 'image/webp'
        # Default
        return 'application/octet-stream'
    return mime


def upload_image(file_path: str, server_base: str, timeout: int, verify_tls: bool) -> str:
    """Uploads the image and returns the URL from the server's JSON response."""
    url = server_base.rstrip('/') + '/deck/upload'
    mime = guess_mime(file_path)

    with open(file_path, 'rb') as f:
        files = {"file": (os.path.basename(file_path), f, mime)}
        resp = requests.post(url, files=files, timeout=timeout, verify=verify_tls)
    # Raise for status so non-2xx returns a proper error
    try:
        resp.raise_for_status()
    except requests.HTTPError as e:
        # Try to include body text for easier debugging
        body_text = ''
        try:
            body_text = resp.text
        except Exception:
            pass
        raise SystemExit(f"HTTP {resp.status_code}: {e}\n{body_text}") from e

    try:
        data = resp.json()
    except ValueError as e:
        raise SystemExit(f"Server did not return JSON. Raw response: {resp.text}") from e

    url_value = data.get('url') if isinstance(data, dict) else None
    if not url_value or not isinstance(url_value, str):
        raise SystemExit(f"Unexpected JSON payload. Expected object with 'url'. Got: {data!r}")

    return url_value


def parse_args(argv: Optional[list[str]] = None) -> argparse.Namespace:
    p = argparse.ArgumentParser(description='Upload a deck image to PokemonTCGPTracker.')
    p.add_argument('image', help='Path to the image file to upload')
    p.add_argument('--server', default='https://localhost:5001', help='Server base URL (default: https://localhost:5001)')
    p.add_argument('--timeout', type=int, default=30, help='HTTP timeout in seconds (default: 30)')
    p.add_argument('--insecure', action='store_true', help='Disable TLS certificate verification (use ONLY for local dev)')
    return p.parse_args(argv)


def main(argv: Optional[list[str]] = None) -> int:
    args = parse_args(argv)

    if not os.path.isfile(args.image):
        print(f"Error: file not found: {args.image}", file=sys.stderr)
        return 1

    try:
        uploaded_url = upload_image(
            file_path=args.image,
            server_base=args.server,
            timeout=args.timeout,
            verify_tls=not args.insecure,
        )
        print(f"Upload OK. Server URL: {uploaded_url}")
        return 0
    except SystemExit as e:
        # Raised intentionally for controlled error messages
        msg = str(e)
        if msg:
            print(msg, file=sys.stderr)
        return 2 if 'HTTP' in msg else 3
    except requests.RequestException as e:
        print(f"Network error: {e}", file=sys.stderr)
        return 3
    except Exception as e:
        print(f"Unexpected error: {e}", file=sys.stderr)
        return 3


if __name__ == '__main__':
    raise SystemExit(main())
