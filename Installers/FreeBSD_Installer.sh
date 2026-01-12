#!/bin/sh

set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
  echo "This script must be run as root. Use sudo or switch to root."
  exit 1
fi

echo "Updating package repository catalog..."
pkg update -f

echo "Bootstrapping pkg if needed..."
if ! command -v pkg >/dev/null 2>&1; then
  env ASSUME_ALWAYS_YES=yes pkg bootstrap
fi

FBSD_PKGS="
libskiasharp-2.88.3_2
lang/dotnet
devel/libepoll-shim
x11/libICE
x11/libSM
x11-fonts/fontconfig
x11-toolkits/libgdiplus
"

echo "Installing FreeBSD packages..."

for p in $FBSD_PKGS; do
  echo "Installing $p ..."
  pkg install -y "$p" || {
    echo "Failed to install $p. Exiting."
    exit 2
  }
done

# Enable and start linux compatibility
echo "Enabling Linux compatibility (linux_enable=YES)..."
sysrc linux_enable="YES"

echo "Starting linux service..."
service linux start

LINUX_PKGS="
linux_base-rl9
linux-rl9-icu-67.1_2
linux-rl9-fontconfig-2.14.0_2
linux-rl9-freetype-2.10.4_3
linux-rl9-wget-1.21.1_1
linux-rl9-ffmpeg-libs-5.1.6_3
linux-rl9-dbus-libs-1.12.20_3
linux-rl9-at-spi2-atk-2.38.0_1
linux-rl9-atk-2.36.0_1
linux-rl9-cups-libs-2.3.3_8
linux-rl9-libxkbcommon-1.0.3_2
linux-rl9-alsa-lib-1.2.13
"

echo "Installing linux-rl9 packages..."
for p in $LINUX_PKGS; do
  echo "Installing $p ..."
  pkg install -y "$p" || {
    echo "Failed to install $p. Exiting."
    exit 3
  }
done

echo "Installing wine..."
if pkg install -y wine; then
  echo "Installed wine."
else
  echo "Failed to install wine. Exiting."
  exit 4
fi

echo "All requested packages installed."

echo "Applying permissions..."
chmod -R a+wx *

echo "Done! You can now run ./PSMultiTools"
