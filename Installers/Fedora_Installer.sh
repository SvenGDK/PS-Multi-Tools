#!/bin/sh

set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
  echo "This script must be run as root. Use sudo or switch to root."
  exit 1
fi

echo "Updating system packages..."
dnf -y update

echo "Installing prerequisites..."
dnf -y install dnf-plugins-core curl wget gnupg2

FEDORA_VER=$(rpm -E %fedora)
echo "Detected Fedora version: $FEDORA_VER"

echo "Adding WineHQ repository for Fedora $FEDORA_VER..."
REPO_URL="https://dl.winehq.org/wine-builds/fedora/${FEDORA_VER}/winehq.repo"
if ! dnf config-manager --add-repo "$REPO_URL"; then
  echo "Failed to add WineHQ repo $REPO_URL. Please verify the URL exists for your Fedora version."
  exit 2
fi

echo "Importing WineHQ GPG key..."
if ! rpm --import https://dl.winehq.org/wine-builds/winehq.key; then
  echo "Failed to import WineHQ GPG key."
  exit 3
fi

echo "Refreshing repository metadata..."
dnf -y makecache

echo "Installing requested packages from Fedora and WineHQ..."
dnf -y install \
  curl gpg wget libICE libSM libX11 fontconfig freetype xorriso libusb1 \
  wine winetricks

DOTNET_NAMES="dotnet-runtime-9.0 dotnet-runtime"
DOTNET_INSTALLED=false

echo "Attempting to install .NET runtime (dotnet-runtime-9.0 preferred)..."
for name in $DOTNET_NAMES; do
  echo "Trying to install $name ..."
  if dnf -y install "$name"; then
    echo "Installed $name via dnf."
    DOTNET_INSTALLED=true
    break
  else
    echo "Failed to install $name via dnf, trying next option if available..."
  fi
done

if [ "$DOTNET_INSTALLED" = false ]; then
  echo "dotnet runtime not found in enabled repos. Attempting to add Microsoft's package repository for Fedora ${FEDORA_VER}..."
  MS_RPM_URL="https://packages.microsoft.com/config/fedora/${FEDORA_VER}/packages-microsoft-prod.rpm"
  TMP_MS_RPM="/tmp/packages-microsoft-prod.rpm"

  if curl -fsSL "$MS_RPM_URL" -o "$TMP_MS_RPM"; then
    if dnf -y install "$TMP_MS_RPM"; then
      echo "Microsoft package repository added. Refreshing metadata..."
      dnf -y makecache
      for name in $DOTNET_NAMES; do
        echo "Retrying install of $name ..."
        if dnf -y install "$name"; then
          echo "Installed $name via dnf (Microsoft repo)."
          DOTNET_INSTALLED=true
          break
        else
          echo "Failed to install $name from Microsoft repo."
        fi
      done
    else
      echo "Failed to install Microsoft packages RPM."
    fi
    rm -f "$TMP_MS_RPM"
  else
    echo "Failed to download Microsoft package repository RPM from $MS_RPM_URL."
  fi
fi

if [ "$DOTNET_INSTALLED" = false ]; then
  echo "Failed to install dotnet runtime (tried: $DOTNET_NAMES). Please install it manually."
fi

echo "Verifying wine installation..."
if command -v wine >/dev/null 2>&1; then
  wine --version || true
else
  echo "Warning: wine command not found after installation."
fi

echo "Verifying dotnet installation..."
if command -v dotnet >/dev/null 2>&1; then
  dotnet --list-runtimes || true
else
  echo "Warning: dotnet command not found after installation."
fi

echo "Applying permissions to current directory..."
chmod -R a+wx *

echo "Done! You can now run ./PSMultiTools"
