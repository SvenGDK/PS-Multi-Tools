#!/bin/sh

set -u

if [ "$(id -u)" -ne 0 ]; then
  echo "This script must be run as root. Use sudo or switch to root."
  exit 1
fi

PKGS="awk wget libice libsm libx11 fontconfig freetype2 ttf-dejavu libglvnd libisoburn libusb wine wine-gecko wine-mono winetricks dotnet-runtime-9.0"

echo "Refreshing package databases..."
pacman -Sy --noconfirm

for p in $PKGS; do
  echo "Installing $p ..."

  if [ "$p" = "dotnet-runtime-9.0" ]; then
    NAMES="dotnet-runtime-9.0 dotnet-runtime"
  else
    NAMES="$p"
  fi

  INSTALLED=false

  for name in $NAMES; do
    if pacman -Qi "$name" >/dev/null 2>&1; then
      echo "$name already installed, skipping."
      INSTALLED=true
      break
    fi

    if pacman -S --noconfirm --needed "$name" >/dev/null 2>&1; then
      echo "Installed $name via pacman."
      INSTALLED=true
      break
    fi

    # If pacman failed, try yay then paru
    if command -v yay >/dev/null 2>&1; then
      echo "pacman failed for $name; attempting to install via yay..."
      if yay -S --noconfirm "$name"; then
        echo "Installed $name via yay."
        INSTALLED=true
        break
      fi
    fi

    if command -v paru >/dev/null 2>&1; then
      echo "pacman failed for $name; attempting to install via paru..."
      if paru -S --noconfirm "$name"; then
        echo "Installed $name via paru."
        INSTALLED=true
        break
      fi
    fi
  done

  if [ "$INSTALLED" = false ]; then
    echo "Failed to install $p (tried: $NAMES) with pacman or available AUR helpers. Please install it manually."
  fi
done

echo "Verifying wine installation..."
if command -v wine >/dev/null 2>&1; then
  wine --version || true
else
  echo "wine not found after installation."
fi

echo "Verifying dotnet installation..."
if command -v dotnet >/dev/null 2>&1; then
  dotnet --list-runtimes || true
else
  echo "dotnet not found after installation."
fi

echo "Applying permissions..."
chmod -R a+wx *

echo "Done! You can now run ./PSMultiTools"
