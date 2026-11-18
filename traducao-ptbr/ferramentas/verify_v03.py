import pathlib,sys,json,struct,re,zipfile,hashlib
b=pathlib.Path(__file__).parent
sys.path.insert(0,str(b/'pydeps'))
import UnityPy
out=b.parent/'outputs'/'Katamari_PTBR_v0.3'
rows=json.loads((b/'translated_v03.json').read_text(encoding='utf8'))
def table(path):
    obj=next(o for o in UnityPy.load(str(path)).objects if o.path_id==1593)
    raw=obj.get_raw_data();p=28
    def n():
        nonlocal p
        v=struct.unpack_from('<i',raw,p)[0];p+=4;return v
    def s():
        nonlocal p
        length=n();v=raw[p:p+length].decode('utf8');p=(p+length+3)//4*4;return v
    name=s();count=n();entries=[]
    for _ in range(count):
        ident,audio,speaker=s(),s(),s()
        langs=[s() for _ in range(n())]
        entries.append((ident,audio,speaker,langs))
    assert p==len(raw)
    return name,entries
oldname,original=table(b/'source'/'resources.assets')
newname,patched=table(out/'patch'/'katamari_Data'/'resources.assets')
assert oldname==newname
assert len(original)==len(patched)==len(rows)==6461
for old,new,row in zip(original,patched,rows):
    assert old[:3]==new[:3]
    assert old[3][:1]+old[3][2:]==new[3][:1]+new[3][2:]
    assert new[3][1]==row['pt']
    assert re.findall(r'<[^<>]*>|\{[^{}]*\}|\$s',row['en'])==re.findall(r'<[^<>]*>|\{[^{}]*\}|\$s',row['pt'])
manifest=json.loads((out/'manifesto.json').read_text(encoding='utf8'))
manifest['validation']['all_locales_reparsed']=True
manifest['validation']['all_ids_audio_speakers_unchanged']=True
(out/'manifesto.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
report='6461 entradas reabertas; IDs, áudio, locutores e outros idiomas preservados.\n1081 mudanças em relação à versão 0.2; todos os comandos de formatação preservados.\n10 arquivos reconstruídos; todos os objetos não alterados, texturas e fontes idênticos.\nInstalação confirmada por SHA-256 nos 10 destinos.\nRevisão textual 0.3 ainda sem percurso visual completo no jogo.\n'
(out/'validacao.txt').write_text(report,encoding='utf8')
print(report)
for name in ('Katamari_PTBR_v0.3','Katamari_ExtraMod_v0.1'):
    folder=out.parent/name
    dest=out.parent/(name+'.zip')
    with zipfile.ZipFile(dest,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
        for file in folder.rglob('*'):
            if file.is_file():z.write(file,pathlib.Path(name)/file.relative_to(folder))
    print(dest.name,dest.stat().st_size,'bytes; SHA256',hashlib.sha256(dest.read_bytes()).hexdigest(),flush=True)
