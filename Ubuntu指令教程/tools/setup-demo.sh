# 在 Ubuntu rootfs 内执行：建演示用户 tom 和演示目录
id tom >/dev/null 2>&1 || useradd -m -s /bin/bash tom
usermod -aG sudo tom
echo 'tom:Tom@2026' | chpasswd
H=/home/tom
mkdir -p $H && cp -r --update=none /etc/skel/. $H/
rm -rf $H/demo && mkdir -p $H/demo/logs $H/demo/src && cd $H/demo
printf 'hello\n' > readme.txt
head -c 2500000 /dev/zero > big.iso
printf 'DB_PASS=123\n' > .env
printf '#!/bin/bash\necho hi\n' > run.sh && chmod +x run.sh
ln -sf readme.txt link.txt
cat > logs/app.log <<'EOF'
2026-10-28 09:00:01 INFO  服务启动
2026-10-28 09:00:05 INFO  连接数据库成功
2026-10-28 09:15:22 WARN  价格表缓存过期，重新加载
2026-10-28 10:02:13 ERROR 订单 A1002 保存失败：超时
2026-10-28 10:02:14 INFO  重试订单 A1002
2026-10-28 10:02:15 INFO  订单 A1002 保存成功
2026-10-28 11:30:40 ERROR 连接数据库失败
2026-10-28 11:30:45 INFO  连接数据库成功
EOF
cat > src/products.csv <<'EOF'
code,name,category,price
U001,探头A,探头,12000
U002,探头B,探头,15000
U003,主机X,主机,280000
U004,凝胶,耗材,35
U005,探头A,探头,12000
EOF
touch -d '2026-10-01 09:00' readme.txt
touch -d '2026-10-20 14:30' big.iso
touch -d '2026-10-28 11:30' logs/app.log
chown -R tom:tom $H
chmod 750 $H
chmod 1777 /tmp
echo setup-ok
