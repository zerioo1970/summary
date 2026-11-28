#!/bin/bash
# run subiquity dry-run TUI inside ubuntu 24.04 base root with the snap runtime
T=/projects/sandbox/.tools
exec bwrap --bind $T/r2404 / --ro-bind $T/iso/snap /snap/subiquity/current --ro-bind $T/r2404 /snap/core24/current --bind $T/subiquity-src /src \
  --dev /dev --proc /proc --tmpfs /tmp --chdir /src --setenv HOME /tmp --setenv TERM linux \
  --setenv LANG C.UTF-8 --setenv PATH /usr/local/sbin:/usr/local/bin:/src/system_scripts:/usr/sbin:/usr/bin:/sbin:/bin --setenv SNAP /snap/subiquity/current \
  --setenv PYTHONPATH /src:/snap/subiquity/current/lib/python3.12/site-packages \
  --setenv LD_LIBRARY_PATH /snap/subiquity/current/usr/lib/x86_64-linux-gnu:/snap/subiquity/current/lib/x86_64-linux-gnu \
  /snap/subiquity/current/usr/bin/python3.12 -m subiquity.cmd.tui --dry-run "$@"
