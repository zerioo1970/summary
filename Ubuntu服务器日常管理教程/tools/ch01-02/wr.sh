#!/bin/bash
# run as root inside the working copy of installed Ubuntu 26.04.1 (hostname web01)
T=/projects/sandbox/.tools
exec bwrap --bind $T/w2604 / --dev /dev --proc /proc --tmpfs /tmp --bind $T/w2604/run /run --unshare-uts --hostname web01 --setenv HOME /root --setenv LANG C.UTF-8 --setenv TERM xterm --setenv PATH /usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin "$@"
