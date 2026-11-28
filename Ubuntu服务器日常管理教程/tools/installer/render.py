# render pyte cell dumps (json) to html that looks like the VMware console
import json,sys,html,os
NAMED={'black':'111111','red':'ff0000','green':'0e8420','brown':'e95420','yellow':'e95420','blue':'007aa6','magenta':'333333','cyan':'808080','white':'ffffff'}
def col(c,default):
    if c=='default': return default
    return NAMED.get(c,c if len(c)==6 else default)
font='file:///projects/sandbox/.tools/fonts/dejavu-fonts-ttf-2.37/ttf/DejaVuSansMono.ttf'
for f in sys.argv[2:]:
    cells=json.load(open(f)); rows=[]
    for r in cells:
        out=[];prev=None;buf=''
        for ch,fg,bg,rev,bold in r:
            a,b=col(fg,'ffffff'),col(bg,'111111')
            if rev: a,b=b,a
            k=(a,b)
            if k!=prev and buf: out.append((prev,buf)); buf=''
            prev=k; buf+=ch or ' '
        out.append((prev,buf))
        rows.append(''.join(f'<span style="color:#{a};background:#{b}">{html.escape(t)}</span>' for (a,b),t in out))
    name=os.path.splitext(os.path.basename(f))[0]
    open(f'{sys.argv[1]}/{name}.html','w').write(f'''<html><head><style>@font-face{{font-family:M;src:url({font})}}
body{{margin:0;background:#111}}#fig{{display:inline-block;padding:10px 12px;background:#111}}
pre{{margin:0;font-family:M;font-size:15px;line-height:1.17;color:#fff}}</style></head><body><div id="fig"><pre>{'<br>'.join(rows)}</pre></div></body></html>''')
