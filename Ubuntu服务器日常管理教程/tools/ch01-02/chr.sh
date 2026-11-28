mkdir -p /run/chrony /var/lib/chrony /var/log/chrony
chronyd -x -u root -f /etc/chrony/chrony.conf </dev/null >/dev/null 2>&1
sleep 40
echo '##### sources'; chronyc sources
echo '##### sources -v'; chronyc sources -v | head -12
echo '##### tracking'; chronyc tracking
echo '##### authdata'; chronyc -N authdata
pkill chronyd
