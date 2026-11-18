import pathlib,struct,json,collections,re
base=pathlib.Path(__file__).parent
raw=(base/'extracted'/'raw_1593.bin').read_bytes()
p=28
def integer():
    global p
    n=struct.unpack_from('<i',raw,p)[0];p+=4;return n
def string():
    global p
    n=integer();start=p;s=raw[p:p+n].decode('utf8');p=(p+n+3)//4*4;return s,start-4
name,_=string();count=integer();rows=[]
for i in range(count):
    text_id,_=string();audio,_=string();speaker,_=string();n=integer()
    assert n==7,(i,n)
    texts=[string() for _ in range(n)]
    rows.append({'id':text_id,'speaker':speaker,'en':texts[1][0],'jp':texts[0][0],'offset':texts[1][1]})
(base/'catalog.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf8')
(base/'catalog_lines.txt').write_text('\n'.join(f"{i}\t{r['id']}\t{json.dumps(r['en'],ensure_ascii=False)}" for i,r in enumerate(rows)),encoding='utf8')
unique=set(r['en'] for r in rows if r['en'].strip())
print('Rows',count,'Unique',len(unique),'Words',sum(len(t.split()) for t in unique),'End',p,'Bytes',len(raw))
print(collections.Counter(re.match('[A-Z_]+',r['id'])[0] if re.match('[A-Z_]+',r['id']) else r['id'] for r in rows).most_common(25))
print('\n'.join((base/'catalog_lines.txt').read_text(encoding='utf8').splitlines()[:65]))
