#!/usr/bin/env bash
set -euo pipefail

URL_LINUX="https://raw.githubusercontent.com/SvenGDK/PS-Multi-Tools/main/LinuxUpdate.zip"
URL_MAC="https://raw.githubusercontent.com/SvenGDK/PS-Multi-Tools/main/macOSUpdate.zip"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

OS="$(uname -s)"
case "$OS" in
  Linux) PLATFORM="linux" ;;
  Darwin) PLATFORM="macos" ;;
  FreeBSD) PLATFORM="freebsd" ;;
  *) PLATFORM="unknown" ;;
esac

if [ "$PLATFORM" = "macos" ]; then
  URL="$URL_MAC"
else
  URL="$URL_LINUX"
fi

if [ "$PLATFORM" = "linux" ]; then
  TMP_ZIP="$(mktemp --tmpdir linuxupdate.XXXXXX)"
elif [ "$PLATFORM" = "macos" ]; then
  TMP_ZIP="$(mktemp -t linuxupdate)"
elif [ "$PLATFORM" = "freebsd" ]; then
  TMP_ZIP="$(mktemp /tmp/linuxupdate.XXXXXX)"
else
  TMP_ZIP="$(mktemp)"
fi

cleanup() {
  rm -f "$TMP_ZIP"
}
trap cleanup EXIT

if ! command -v unzip >/dev/null 2>&1; then
  if [ "$PLATFORM" = "linux" ]; then
    echo "Error: unzip is not installed. Install it (e.g., sudo apt install unzip) and retry." >&2
  elif [ "$PLATFORM" = "macos" ]; then
    echo "Error: unzip is not available. On macOS install via Homebrew if needed: brew install unzip" >&2
  elif [ "$PLATFORM" = "freebsd" ]; then
    echo "Error: unzip is not installed. Install it with: sudo pkg install unzip" >&2
  else
    echo "Error: unzip is not installed. Please install unzip and retry." >&2
  fi
  exit 1
fi

echo "Platform detected: $PLATFORM"
echo "Downloading $URL ..."

if command -v curl >/dev/null 2>&1; then
  curl -fL --retry 3 --retry-delay 2 -o "$TMP_ZIP" "$URL"
elif command -v wget >/dev/null 2>&1; then
  wget -q -O "$TMP_ZIP" "$URL"
elif [ "$PLATFORM" = "freebsd" ] && command -v fetch >/dev/null 2>&1; then
  fetch -o "$TMP_ZIP" "$URL"
else
  echo "Error: neither curl nor wget (nor fetch on FreeBSD) is available. Install one and retry." >&2
  exit 1
fi

if command -v file >/dev/null 2>&1; then
  if ! file -b --mime-type "$TMP_ZIP" | grep -qi 'zip'; then
    echo "Warning: downloaded file does not appear to be a ZIP archive." >&2
  fi
fi

echo "Extracting into $SCRIPT_DIR ..."
unzip -o "$TMP_ZIP" -d "$SCRIPT_DIR"

echo "Done. PS Multi Tools has been updated!"
