import json,sys
for f in sys.argv[1:]:
    c=json.load(open(f)); out=[]
    for y,r in enumerate(c[3:],3):
        hl=[x for x in r if x[2] not in ('black','default')]
        if hl: out.append(f"{y}:{''.join(x[0] for x in hl).strip()[:50]}({hl[0][2]})")
    print(f, out)
