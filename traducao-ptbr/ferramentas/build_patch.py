import sys,pathlib,json,struct,hashlib,collections,re,csv
base=pathlib.Path(__file__).parent
sys.path.insert(0,str(base/'pydeps'))
import UnityPy
out=base.parent/'outputs'/'Katamari_PTBR_v0.1'
out.mkdir(parents=True,exist_ok=True)
source=base/'source'/'resources.assets'
rows=json.loads((base/'translated.json').read_text(encoding='utf8'))
original=UnityPy.load(str(source));target=next(o for o in original.objects if o.path_id==1593)
raw=target.get_raw_data();updated=raw
controls=r'\{[^{}]*\}|<[^<>]*>|\$s'
for r in reversed(rows):
    assert re.findall(controls,r['en'])==re.findall(controls,r['pt']),r['id']
    off=r['offset'];length=struct.unpack_from('<i',raw,off)[0]
    assert raw[off+4:off+4+length].decode('utf8')==r['en']
    end=(off+4+length+3)//4*4
    b=r['pt'].encode('utf8');replacement=struct.pack('<i',len(b))+b+b'\0'*((-len(b))%4)
    updated=updated[:off]+replacement+updated[end:]
target.set_raw_data(updated)
dest=out/'resources.assets';dest.write_bytes(original.file.save())
reloaded=UnityPy.load(str(dest))
srcobjs={o.path_id:o for o in UnityPy.load(str(source)).objects}
dstobjs={o.path_id:o for o in reloaded.objects}
assert srcobjs.keys()==dstobjs.keys()
assert all(srcobjs[i].get_raw_data()==dstobjs[i].get_raw_data() for i in srcobjs if i!=1593)
# Reparse the full edited table, checking every locale, speaker and audio reference.
def parse(data):
    p=28
    def n():
        nonlocal p
        value=struct.unpack_from('<i',data,p)[0];p+=4;return value
    def s():
        nonlocal p
        length=n();v=data[p:p+length].decode('utf8');p=(p+length+3)//4*4;return v
    name=s();count=n();records=[]
    for _ in range(count):
        tid=s();audio=s();speaker=s();langs=[s() for _ in range(n())]
        records.append((tid,audio,speaker,langs))
    assert p==len(data)
    return name,records
oldname,old=parse(raw);newname,new=parse(dstobjs[1593].get_raw_data())
assert oldname==newname and len(old)==len(new)==len(rows)
for a,b,r in zip(old,new,rows):
    assert a[:3]==b[:3]
    assert a[3][:1]+a[3][2:]==b[3][:1]+b[3][2:]
    assert b[3][1]==r['pt']
manifest={'version':'0.1','language':'pt-BR','base_locale':'English','game':'Katamari Damacy REROLL','steam_buildid':'20629440','source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'patch_sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'table_entries':len(rows),'changed_entries':sum(r['en']!=r['pt'] for r in rows),'methods':dict(collections.Counter(r['method'] for r in rows)),'validation':{'unchanged_other_objects':len(srcobjs)-1,'all_locales_reparsed':True,'all_control_tokens_preserved':True},'limitations':['Primeira versão com tradução automática parcialmente revisada.','Texturas, vídeos, nomes de músicas, créditos e contrato de licença não foram traduzidos.','Validação visual limitada ao início do jogo e menus acessíveis.']}
(out/'manifesto.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
with (out/'textos.tsv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.writer(f,delimiter='\t');w.writerow(['ID','Inglês','Português brasileiro','Origem'])
    w.writerows((r['id'],r['en'],r['pt'],r['method']) for r in rows)
print(json.dumps(manifest,ensure_ascii=False,indent=2),flush=True)
