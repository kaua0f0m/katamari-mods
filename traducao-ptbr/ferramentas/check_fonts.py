import sys,pathlib,struct
sys.path.insert(0,str(pathlib.Path(__file__).parent/'pydeps'))
import UnityPy
def covered(b):
    n=struct.unpack_from('>H',b,4)[0];chars=set()
    records=[struct.unpack_from('>4sIII',b,12+i*16) for i in range(n)]
    cm=next(x[2] for x in records if x[0]==b'cmap')
    count=struct.unpack_from('>H',b,cm+2)[0]
    for i in range(count):
        platform,enc,off=struct.unpack_from('>HHI',b,cm+4+i*8);p=cm+off
        if platform not in [0,3]:continue
        fmt=struct.unpack_from('>H',b,p)[0]
        if fmt==12:
            ng=struct.unpack_from('>I',b,p+12)[0]
            for j in range(ng):
                start,end,glyph=struct.unpack_from('>III',b,p+16+j*12)
                if glyph:chars.update(range(start,end+1))
        elif fmt==4:
            seg=struct.unpack_from('>H',b,p+6)[0]//2
            for j in range(seg):
                end=struct.unpack_from('>H',b,p+14+2*j)[0]
                start=struct.unpack_from('>H',b,p+16+2*seg+2*j)[0]
                delta=struct.unpack_from('>h',b,p+16+4*seg+2*j)[0]
                rop=p+16+6*seg+2*j;ro=struct.unpack_from('>H',b,rop)[0]
                for c in range(start,end+1):
                    glyph=(c+delta)%65536 if not ro else struct.unpack_from('>H',b,rop+ro+2*(c-start))[0]
                    if glyph:chars.add(c)
    return chars
env=UnityPy.load(str(pathlib.Path(__file__).parent/'source'/'resources.assets'))
for o in env.objects:
    if o.type.name=='Font':
        d=o.parse_as_object();cs=covered(bytes(d.m_FontData))
        missing=''.join(c for c in 'áàâãéêíóôõúüçÁÀÂÃÉÊÍÓÔÕÚÜÇºª' if ord(c) not in cs)
        print(d.m_Name,'Missing:',missing or 'none',flush=True)
