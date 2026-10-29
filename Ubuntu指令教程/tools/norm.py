#!/usr/bin/env python3
# 把沙箱里 ls -l 风格的输出规整成普通 Ubuntu(ext4) 上的样子：
# 1) 去掉 SELinux 宿主导致的权限后多余的 "+"
# 2) 目录大小按 ext4 显示为 4096 / 4.0K
# 3) 按 GNU ls 的方式重新对齐各列
import re, sys

PAT = re.compile(r'^([-dlcbps][rwxsStT-]{9})\+?\s+(\d+)\s+(\S+)\s+(\S+)\s+(\S+)\s+([A-Z][a-z]{2})\s+(\d+)\s+(\S+)\s(.*)$')
PAT_ISO = re.compile(r'^([-dlcbps][rwxsStT-]{9})\+?\s+(\d+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\d{4}-\d\d-\d\d)\s+(\d\d:\d\d\S*)\s(.*)$')

def main():
    rows = []
    for line in sys.stdin.buffer.read().decode('utf-8', 'replace').split('\n'):
        if 'tcsetattr: Inappropriate ioctl' in line or line.startswith('bash: cannot set terminal process group') or line == 'bash: no job control in this shell':
            continue
        m = PAT.match(line)
        iso = None if m else PAT_ISO.match(line)
        if m:
            mode, n, u, g, size, mon, day, tm, name = m.groups()
            date = (mon, day, tm)
        elif iso:
            mode, n, u, g, size, d, tm, name = iso.groups()
            date = (d, tm)
        else:
            rows.append(('raw', line))
            continue
        rows.append(('ls', mode, n, u, g, size, date, name.lstrip()))
    out, block = [], []
    def flush():
        if not block:
            return
        human = any(re.search(r'[KMGT]$', r[5]) for r in block)
        for i, r in enumerate(block):
            if r[1].startswith('d'):
                block[i] = r[:5] + (('4.0K' if human else '4096'),) + r[6:]
        wn = max(len(r[2]) for r in block)
        wu = max(len(r[3]) for r in block)
        wg = max(len(r[4]) for r in block)
        ws = max(len(r[5]) for r in block)
        wd = max(len(r[6][1]) for r in block) if len(block[0][6]) == 3 else 0
        wt = max(len(r[6][-1]) for r in block)
        for r in block:
            date = r[6]
            if len(date) == 3:
                ds = f'{date[0]} {date[1].rjust(2)} {date[2].rjust(5)}'
            else:
                ds = f'{date[0]} {date[1]}'
            out.append(f'{r[1]} {r[2].rjust(wn)} {r[3].ljust(wu)} {r[4].ljust(wg)} {r[5].rjust(ws)} {ds} {r[7]}')
        block.clear()
    for r in rows:
        if r[0] == 'ls':
            if block and len(block[-1][6]) != len(r[6]):
                flush()
            block.append(r)
        else:
            flush()
            out.append(r[1])
    flush()
    sys.stdout.write('\n'.join(out))

main()
