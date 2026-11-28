ip link set lo up; ip addr add 192.168.1.50/24 dev lo
mkdir -p /run/sshd; /usr/sbin/sshd -E /tmp/sshd.log </dev/null >/dev/null 2>&1
rm -rf /home/zhangw/.ssh; mkdir -p /home/zhangw/.ssh; ssh-keyscan -t ed25519 192.168.1.50 2>/dev/null > /home/zhangw/.ssh/known_hosts; chown -R zhangw /home/zhangw/.ssh
timeout 90 su zhangw -c "cd ~ && python3 /opt/t/sshtest.py badpw" </dev/null
tail -4 /tmp/sshd.log
