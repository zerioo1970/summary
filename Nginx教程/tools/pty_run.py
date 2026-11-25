#!/usr/bin/env python3
"""在真实终端（pty）里驱动交互式命令，用来实测 ssh 首次连接提示、输入密码等界面。
用法：python3 pty_run.py 剧本.json
剧本：{"setup": "进入沙箱后先执行的命令（不显示）", "steps": [["等待出现的文字", "要输入的内容"], ...]}
输出：终端里实际显示的全部文字（密码不会回显，和真实终端一样）。
"""
import json, os, pty, re, select, sys, time
R = os.environ.get("UROOT", "/projects/sandbox/.tools/u2604")
spec = json.load(open(sys.argv[1], encoding="utf-8"))
inner = spec["setup"] + "\nexec " + spec.get("shell", "bash --norc -i")
cmd = ["bwrap", "--unshare-net", "--clearenv", "--unshare-pid", "--die-with-parent", "--unshare-uts",
       "--hostname", "web01", "--bind", R, "/", "--dev", "/dev", "--proc", "/proc", "--ro-bind", "/sys", "/sys",
       "--tmpfs", "/run", "--tmpfs", "/tmp", "--setenv", "HOME", "/root", "--setenv", "TERM", "xterm",
       "--setenv", "LANG", "C.UTF-8", "--setenv", "PATH", "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin",
       "/bin/bash", "-c", inner]
pid, fd = pty.fork()
if pid == 0:
    os.execvp(cmd[0], cmd)
buf = ""
def read(timeout):
    global buf
    end = time.time() + timeout
    while time.time() < end:
        r, _, _ = select.select([fd], [], [], 0.1)
        if r:
            try: data = os.read(fd, 4096)
            except OSError: return False
            if not data: return False
            buf += data.decode("utf-8", "replace")
    return True
pos = 0
for want, send in spec["steps"]:
    end = time.time() + 15
    while time.time() < end and not re.search(want, buf[pos:]):
        if not read(0.2): break
    m = re.search(want, buf[pos:])
    if m: pos += m.end()
    else: buf += f"\n[没等到：{want}]\n"
    time.sleep(0.3)
    os.write(fd, send.encode())
read(2)
out = re.sub(r"\x1b\][^\x07\x1b]*(?:\x07|\x1b\\)", "", buf)     # 去掉终端标题等 OSC 控制序列
out = re.sub(r"\x1b\[[?0-9;]*[a-zA-Z]", "", out)                 # 去掉颜色、括号粘贴模式等 CSI 控制序列
out = "\n".join(l for l in out.replace("\r\n", "\n").replace("\r", "").split("\n") if "unix_chkpwd" not in l)  # 这一行是沙箱里 sshd 打到同一终端的日志
sys.stdout.write(out)
