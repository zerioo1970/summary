import os,pty,sys,time,select,subprocess
def run(cmd, script):
    pid,fd=pty.fork()
    if pid==0:
        os.execvp(cmd[0],cmd)
    out=b''
    def pump(t):
        nonlocal out
        end=time.time()+t
        while time.time()<end:
            r,_,_=select.select([fd],[],[],0.1)
            if r:
                try: d=os.read(fd,4096)
                except OSError: return False
                if not d: return False
                out+=d
        return True
    pos=0
    for wait,send in script:
        end=time.time()+15
        while wait.encode() not in out[pos:] and time.time()<end:
            if not pump(0.2): break
        pump(0.3)
        pos=len(out)
        if send is not None: os.write(fd,send.encode())
    pump(2)
    try: os.waitpid(pid,0)
    except: pass
    return out.decode(errors='replace').replace('\r','')
mode=sys.argv[1]
if mode=='first':
    print(run(['ssh','ubuntu@192.168.1.50'],[('(yes/no/[fingerprint])?','yes\n'),('password:','Passw0rd\n'),('$ ','hostname\n'),('$ ','exit\n')]))
elif mode=='again':
    print(run(['ssh','ubuntu@192.168.1.50'],[('password:','Passw0rd\n'),('$ ','exit\n')]))
elif mode=='badpw':
    print(run(['ssh','ubuntu@192.168.1.50'],[('password:','wrong1\n'),('password:','wrong2\n'),('password:','wrong3\n')]))
elif mode=='cmd':
    print(run(['ssh','ubuntu@192.168.1.50','uptime'],[('password:','Passw0rd\n')]))
