# drive subiquity TUI in a pty; steps json: [{"wait":"text"},{"keys":"\r"},{"shot":"name"},{"sleep":1}]
import os,pty,sys,json,time,select,fcntl,termios,struct
sys.path.insert(0,'/projects/sandbox/.tools/pylib'); import pyte
COLS,ROWS=int(os.environ.get('COLS',100)),int(os.environ.get('ROWS',30))
steps=json.load(open(sys.argv[1])); outdir=sys.argv[2]; os.makedirs(outdir,exist_ok=True)
scr=pyte.Screen(COLS,ROWS); st=pyte.ByteStream(scr)
pid,fd=pty.fork()
if pid==0:
    os.execv('/projects/sandbox/.tools/sq.sh',['sq.sh','--machine-config','examples/machines/vmware.json','--bootloader',os.environ.get('BOOT','uefi'),'--source-catalog','examples/sources/iso2604.yaml','--dry-run-config','examples/dry-run-configs/kiro.yaml']+sys.argv[3:])
fcntl.ioctl(fd,termios.TIOCSWINSZ,struct.pack('HHHH',ROWS,COLS,0,0))
def pump(t):
    end=time.time()+t
    while time.time()<end:
        r,_,_=select.select([fd],[],[],0.1)
        if r:
            try: st.feed(os.read(fd,65536))
            except OSError: return
def text(): return "\n".join(scr.display)
def dump(name):
    cells=[[ (c.data,c.fg,c.bg,c.reverse,c.bold) for c in (scr.buffer[y][x] for x in range(COLS))] for y in range(ROWS)]
    json.dump(cells,open(f'{outdir}/{name}.json','w'),ensure_ascii=False)
    open(f'{outdir}/{name}.txt','w').write(text())
pump(2)
for s in steps:
    if 'if' in s and s['if'] not in text(): continue
    if 'waitany' in s:
        end=time.time()+s.get('t',60)
        while not any(w in text() for w in s['waitany']) and time.time()<end: pump(0.3)
        pump(s.get('settle',1.0))
    if 'wait' in s:
        end=time.time()+s.get('t',60)
        while s['wait'] not in text() and time.time()<end: pump(0.3)
        if s['wait'] not in text(): print('TIMEOUT waiting',s['wait']); dump('timeout'); break
        pump(s.get('settle',1.0))
    if 'tabto' in s:
        def focused():
            return ' '.join(''.join(c.data for c in scr.buffer[y].values() if c.bg not in ('black','default')) for y in range(3,ROWS))
        n=0
        while s['tabto'] not in focused() and n<40:
            os.write(fd,s.get('key','\t').encode()); pump(0.35); n+=1
        if s['tabto'] not in focused(): print('TABTO FAIL',s['tabto']); dump('tabfail')
    if 'keys' in s:
        for k in s['keys'] if isinstance(s['keys'],list) else [s['keys']]:
            os.write(fd,k.encode()); pump(s.get('gap',0.4))
    if 'sleep' in s: pump(s['sleep'])
    if 'shot' in s: dump(s['shot']); print('shot',s['shot'])
os.kill(pid,9)
