#!/bin/sh

set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
  echo "Run this script as root or with sudo."
  exit 1
fi

export DEBIAN_FRONTEND=noninteractive

echo "Updating package lists..."
apt-get update -y

echo "Installing prerequisites..."
apt-get install -y --no-install-recommends \
  curl gnupg wget ca-certificates lsb-release software-properties-common

echo "Enabling 32-bit architecture..."
dpkg --add-architecture i386 || true

echo "Adding WineHQ GPG key..."
mkdir -p /usr/share/keyrings
wget -qO- https://dl.winehq.org/wine-builds/winehq.key | gpg --dearmor > /usr/share/keyrings/winehq-archive.gpg

CODENAME=$(lsb_release -cs)
echo "Detected distro codename: $CODENAME"

echo "Adding WineHQ repository for $CODENAME..."
echo "deb [signed-by=/usr/share/keyrings/winehq-archive.gpg] https://dl.winehq.org/wine-builds/ubuntu/ $CODENAME main" \
  > /etc/apt/sources.list.d/winehq.list

echo "Updating package lists after adding WineHQ..."
apt-get update -y

echo "Installing requested packages and WineHQ stable..."
apt-get install -y --install-recommends \
  curl gnupg wget libice6 libsm6 libx11-6 libfontconfig1 xorriso libusb-1.0-0 \
  winehq-stable winetricks

DOTNET_NAMES="dotnet-runtime-9.0 dotnet-runtime"
DOTNET_INSTALLED=false

echo "Attempting to install .NET runtime (dotnet-runtime-9.0 preferred)..."
for name in $DOTNET_NAMES; do
  echo "Trying to install $name ..."
  if apt-get update -y && apt-get install -y --no-install-recommends "$name"; then
    echo "Installed $name via apt."
    DOTNET_INSTALLED=true
    break
  else
    echo "Failed to install $name from current apt sources; trying next option if available..."
  fi
done

if [ "$DOTNET_INSTALLED" = false ]; then
  echo "dotnet runtime not found in enabled repos. Attempting to add Microsoft's package repository..."
  MS_DEB_URL="https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb"
  TMP_MS_DEB="/tmp/packages-microsoft-prod.deb"

  if curl -fsSL "$MS_DEB_URL" -o "$TMP_MS_DEB"; then
    if dpkg -i "$TMP_MS_DEB"; then
      echo "Microsoft package repository added (Ubuntu release path). Refreshing metadata..."
      apt-get update -y
      for name in $DOTNET_NAMES; do
        echo "Retrying install of $name ..."
        if apt-get install -y --no-install-recommends "$name"; then
          echo "Installed $name via apt (Microsoft repo)."
          DOTNET_INSTALLED=true
          break
        else
          echo "Failed to install $name from Microsoft repo."
        fi
      done
    else
      echo "Failed to install Microsoft packages DEB from $TMP_MS_DEB."
    fi
    rm -f "$TMP_MS_DEB"
  else
    MS_DEB_URL2="https://packages.microsoft.com/config/ubuntu/${CODENAME}/packages-microsoft-prod.deb"
    if curl -fsSL "$MS_DEB_URL2" -o "$TMP_MS_DEB"; then
      if dpkg -i "$TMP_MS_DEB"; then
        echo "Microsoft package repository added (codename path). Refreshing metadata..."
        apt-get update -y
        for name in $DOTNET_NAMES; do
          echo "Retrying install of $name ..."
          if apt-get install -y --no-install-recommends "$name"; then
            echo "Installed $name via apt (Microsoft repo)."
            DOTNET_INSTALLED=true
            break
          else
            echo "Failed to install $name from Microsoft repo."
          fi
        done
      else
        echo "Failed to install Microsoft packages DEB from $TMP_MS_DEB."
      fi
      rm -f "$TMP_MS_DEB"
    else
      echo "Failed to download Microsoft package repository DEB from both release and codename URLs."
    fi
  fi
fi

if [ "$DOTNET_INSTALLED" = false ]; then
  echo "Failed to install dotnet runtime (tried: $DOTNET_NAMES). Please install it manually."
fi

echo "Verifying wine installation..."
if command -v wine >/dev/null 2>&1; then
  wine --version || true
else
  echo "wine not found or failed to run"
fi

echo "Verifying dotnet installation..."
if command -v dotnet >/dev/null 2>&1; then
  dotnet --list-runtimes || true
else
  echo "dotnet not found or failed to run"
fi

echo "Applying permissions to current directory..."
chmod -R a+wx *

echo "Done! You can now run ./PSMultiTools"
