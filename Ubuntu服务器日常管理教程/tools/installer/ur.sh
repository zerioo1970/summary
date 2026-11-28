#!/bin/bash
# run a command inside the Ubuntu 26.04.1 rootfs built from the ISO
T=/projects/sandbox/.tools
exec bwrap --bind $T/u2604 / --dev /dev --proc /proc --tmpfs /tmp --tmpfs /run --unshare-uts --hostname web01 --setenv HOME /root --setenv LANG C.UTF-8 --setenv TERM xterm "$@"
