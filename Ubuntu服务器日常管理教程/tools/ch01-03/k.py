import os,pty,sys,time,select
def run(cmd, script, stdin_data=None):
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
        pump(0.3); pos=len(out)
        if send is not None: os.write(fd,send.encode())
    pump(2)
    try: os.waitpid(pid,0)
    except: pass
    return out.decode(errors='replace').replace('\r','')
m=sys.argv[1]
if m=='keygen':
    print(run(['ssh-keygen','-t','ed25519','-C','zhang@zhang-pc'],[('save the key','\n'),('passphrase','\n'),('again','\n')]))
elif m=='pw':  # ssh with password, extra args
    print(run(['ssh']+sys.argv[2:],[('password:','Passw0rd\n')]))
