#!/bin/sh
set -eu

if ! command -v winetricks >/dev/null 2>&1; then
  echo "Error: winetricks not found in PATH. Install winetricks and re-run." >&2
  exit 2
fi

winetricks vcrun2008 vcrun2010 vcrun2012

echo "winetricks finished. Check output above for any errors."
