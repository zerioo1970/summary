ip link set lo up; ip addr add 192.168.1.50/24 dev lo
rm -f /etc/ssh/ssh_host_*; ssh-keygen -A >/dev/null; rm -f /var/lib/wtmpdb/* /var/log/wtmp /var/log/lastlog 2>/dev/null
mkdir -p /run/sshd; /usr/sbin/sshd -E /tmp/sshd.log </dev/null >/dev/null 2>&1
rm -rf /home/zhangw/.ssh
t(){ echo "##### $1"; timeout 40 su zhangw -c "cd ~ && python3 /opt/t/sshtest.py $1" </dev/null; }
t first; t again; t badpw; t cmd
echo "##### fingerprint on server"; ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub
pkill sshd; rm -f /etc/ssh/ssh_host_*; ssh-keygen -A >/dev/null; /usr/sbin/sshd -E /tmp/sshd2.log </dev/null >/dev/null 2>&1
t again
echo "##### refused"; pkill sshd; sleep 1; timeout 10 su zhangw -c "ssh ubuntu@192.168.1.50" </dev/null
