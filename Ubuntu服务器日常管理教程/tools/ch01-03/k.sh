chmod 1777 /tmp
ip link set lo up; ip addr add 192.168.1.50/24 dev lo
rm -f /etc/ssh/sshd_config.d/*; printf 'PasswordAuthentication yes\n' > /etc/ssh/sshd_config.d/50-cloud-init.conf; chmod 600 /etc/ssh/sshd_config.d/50-cloud-init.conf
rm -rf /home/ubuntu/.ssh /home/zhangw/.ssh; mkdir -p /run/sshd
/usr/sbin/sshd -E /tmp/sshd.log </dev/null >/dev/null 2>&1
mkdir -p /home/zhangw/.ssh; ssh-keyscan -t ed25519 192.168.1.50 2>/dev/null > /home/zhangw/.ssh/known_hosts; chown -R zhangw /home/zhangw/.ssh; chmod 700 /home/zhangw/.ssh
Z(){ timeout 40 su zhangw -c "cd ~ && $1" </dev/null; }
echo "##### keygen"; Z "python3 /opt/t/k.py keygen"
echo "##### ls"; Z "ls -l ~/.ssh"; Z "cat ~/.ssh/id_ed25519.pub"
echo "##### copy (CRLF like Windows PowerShell 5.1)"; Z "sed 's/\$/\r/' ~/.ssh/id_ed25519.pub > /tmp/pub.crlf; python3 -c \"
import pty,os,sys,time,select,subprocess
\"; true"
# pipe pubkey with CRLF into ssh, password via SSH_ASKPASS
cat > /tmp/ap.sh <<'A'
#!/bin/sh
echo Passw0rd
A
chmod 755 /tmp/ap.sh
Z "SSH_ASKPASS=/tmp/ap.sh SSH_ASKPASS_REQUIRE=force ssh ubuntu@192.168.1.50 'umask 077; mkdir -p .ssh; cat >> .ssh/authorized_keys' < /tmp/pub.crlf; echo rc=\$?"
echo "##### server authorized_keys"; ls -ld /home/ubuntu/.ssh; ls -l /home/ubuntu/.ssh; od -c /home/ubuntu/.ssh/authorized_keys | tail -3
echo "##### key login"; Z "ssh ubuntu@192.168.1.50 hostname"
echo "##### key login -v"; Z "ssh -v ubuntu@192.168.1.50 true 2>&1 | grep -E 'Authentications that can continue|Offering public key|Server accepts key|Authenticated to'"
echo "##### config"; printf 'Host web01\n    HostName 192.168.1.50\n    User ubuntu\n' > /home/zhangw/.ssh/config; chown zhangw /home/zhangw/.ssh/config; Z "ssh web01 hostname"
echo "##### sshd -T before"; sshd -T 2>/dev/null | grep -E '^(passwordauthentication|pubkeyauthentication|permitrootlogin|kbdinteractiveauthentication)'
echo "##### wrong: edit main config only"; cp /etc/ssh/sshd_config /tmp/sshd_config.bak; sed -i 's/^#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config; grep -n '^PasswordAuthentication' /etc/ssh/sshd_config; sshd -T | grep '^passwordauthentication'; cp /tmp/sshd_config.bak /etc/ssh/sshd_config
echo "##### dropin typo"; printf 'PasswordAuthentication nope\n' > /etc/ssh/sshd_config.d/01-no-password.conf; sshd -t; echo rc=$?
echo "##### dropin ok"; printf 'PasswordAuthentication no\n' > /etc/ssh/sshd_config.d/01-no-password.conf; sshd -t; echo rc=$?; ls -l /etc/ssh/sshd_config.d/; sshd -T | grep '^passwordauthentication'
pkill -f 'sshd -E'; sleep 1; /usr/sbin/sshd -E /tmp/sshd2.log </dev/null >/dev/null 2>&1; sleep 1
echo "##### password login refused"; Z "ssh -o PubkeyAuthentication=no ubuntu@192.168.1.50"
echo "##### key still ok"; Z "ssh web01 hostname"
echo "##### bad perms"; chmod 777 /home/ubuntu/.ssh; Z "ssh web01 hostname"; tail -3 /tmp/sshd2.log; chmod 700 /home/ubuntu/.ssh
echo "##### no key on client"; Z "mv ~/.ssh/id_ed25519 /tmp/k; ssh web01 hostname; mv /tmp/k ~/.ssh/id_ed25519"
echo "##### done"
