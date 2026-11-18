import pathlib,sys,struct,json,re
base=pathlib.Path(__file__).parent
def read_table(raw,kind):
    p=28
    def n():
        nonlocal p
        x=struct.unpack_from('<i',raw,p)[0];p+=4;return x
    def s():
        nonlocal p
        off=p;length=n();x=raw[p:p+length].decode('utf8');p=(p+length+3)//4*4;return off,x
    name=s();records=[]
    if kind=='eula':
        sheets=n()
        for _ in range(sheets):
            sheet=s();count=n()
            for i in range(count):
                tid=s();langs=[s() for _ in range(7)]
                records.append({'object':1591,'id':tid[1],'offset':langs[1][0],'en':langs[1][1]})
    else:
        count=n()
        for _ in range(count):s()
        count=n()
        for i in range(count):
            off,text=s();records.append({'object':1594,'id':f'CREDIT_{i:04d}','offset':off,'en':text})
    assert p==len(raw),(kind,p,len(raw))
    return records
if __name__=='__main__':
    rows=read_table((base/'extracted'/'raw_1591.bin').read_bytes(),'eula')+read_table((base/'extracted'/'raw_1594.bin').read_bytes(),'credits')
    (base/'extra_catalog.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf8')
    print('EULA',sum(r['object']==1591 for r in rows),'Credits',sum(r['object']==1594 for r in rows),'words',sum(len(r['en'].split()) for r in rows if r['object']==1591))
