# interactive bash driver: python3 tty.py steps.json outdir ; screen snapshots as txt
import os,pty,sys,json,time,select,fcntl,termios,struct
sys.path.insert(0,'/projects/sandbox/.tools/pylib'); import pyte
COLS,ROWS=int(os.environ.get('COLS',90)),int(os.environ.get('ROWS',24))
steps=json.load(open(sys.argv[1])); out=sys.argv[2]; os.makedirs(out,exist_ok=True)
scr=pyte.Screen(COLS,ROWS); st=pyte.ByteStream(scr)
pid,fd=pty.fork()
if pid==0:
    os.environ['TERM']='xterm'
    os.execv('/projects/sandbox/.tools/wr.sh',['wr.sh','su','-','ubuntu'])
fcntl.ioctl(fd,termios.TIOCSWINSZ,struct.pack('HHHH',ROWS,COLS,0,0))
def pump(t):
    end=time.time()+t
    while time.time()<end:
        r,_,_=select.select([fd],[],[],0.05)
        if r:
            try: st.feed(os.read(fd,65536))
            except OSError: return
pump(2)
for s in steps:
    if 'k' in s:
        for ch in s['k']:
            os.write(fd,ch.encode()); pump(s.get('gap',0.05))
    pump(s.get('w',0.8))
    if 'shot' in s:
        open(f"{out}/{s['shot']}.txt",'w').write("\n".join(l.rstrip() for l in scr.display).rstrip()+"\n")
    if s.get('clear'): os.write(fd,b'\x0c'); pump(0.5)
os.kill(pid,9)
