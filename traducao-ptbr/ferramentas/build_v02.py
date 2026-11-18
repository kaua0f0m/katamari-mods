import pathlib,sys,json,struct,re,hashlib,csv,collections
base=pathlib.Path(__file__).parent
sys.path.insert(0,str(base/'pydeps'))
import UnityPy
from extra_resources import read_table
out=base.parent/'outputs'/'Katamari_PTBR_v0.2'
out.mkdir(parents=True,exist_ok=True)
game=pathlib.Path(r'D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL')
rows=json.loads((base/'translated_v02.json').read_text(encoding='utf8'))
extra=json.loads((base/'extra_translated.json').read_text(encoding='utf8'))
static=json.loads((base/'static_translated.json').read_text(encoding='utf8'))
original_source=base/'source'/'resources.assets'
changes=collections.defaultdict(lambda:collections.defaultdict(list))
for r in rows:
    if r['en']!=r['pt']:changes[str(original_source)][('resources.assets',1593)].append((r['offset'],r['en'],r['pt']))
for r in extra:
    if r['en']!=r['pt']:changes[str(original_source)][('resources.assets',r['object'])].append((r['offset'],r['en'],r['pt']))
for r in static:changes[r['file']][(r['asset'],r['object'])].append((r['offset'],r['text'],r['pt']))
v01=json.loads((out.parent/'Katamari_PTBR_v0.1'/'manifesto.json').read_text(encoding='utf-8-sig'))
manifest={'version':'0.2','language':'pt-BR','base_locale':'English','steam_buildid':'20629440','files':[],'main_entries':len(rows),'main_changed':sum(r['en']!=r['pt'] for r in rows),'additional_changed':sum(r['en']!=r['pt'] for r in extra),'static_changed':len(static),'validation':{'all_modified_strings_reparsed':True,'unchanged_objects_byte_identical':True,'textures_and_fonts_byte_identical':True},'limitations':['Tradução automática parcialmente revisada.','Texturas e textos gravados nos vídeos permanecem originais.','Nomes próprios, marcas, termos culturais e algumas saudações estrangeiras foram preservados.','A versão 0.2 ainda não foi percorrida integralmente no jogo.']}
def sha(b):return hashlib.sha256(b).hexdigest()
def patch(raw,edits):
    value=raw
    for off,en,pt in sorted(edits,reverse=True):
        assert re.findall(r'<[^<>]*>|\{[^{}]*\}|\$s',en)==re.findall(r'<[^<>]*>|\{[^{}]*\}|\$s',pt),(en[:100],pt[:100])
        n=struct.unpack_from('<i',raw,off)[0]
        assert raw[off+4:off+4+n].decode('utf8')==en
        end=(off+4+n+3)//4*4;b=pt.encode('utf8')
        value=value[:off]+struct.pack('<i',len(b))+b+b'\0'*((-len(b))%4)+value[end:]
    return value
def verify_strings(raw,edits):
    delta=0
    for off,en,pt in sorted(edits):
        p=off+delta;n=struct.unpack_from('<i',raw,p)[0]
        assert raw[p+4:p+4+n].decode('utf8')==pt
        delta+=((4+len(pt.encode('utf8'))+3)//4*4)-((4+len(en.encode('utf8'))+3)//4*4)
for file,objects in changes.items():
    src=pathlib.Path(file)
    if str(src).startswith(str(game)):rel=src.relative_to(game);src=base/'sources_v02'/rel
    else:rel=pathlib.Path('katamari_Data')/src.name
    assert src.exists(),src
    source_bytes=src.read_bytes();env=UnityPy.load(str(src))
    orig_objects={(o.assets_file.name,o.path_id):o.get_raw_data() for o in env.objects}
    editedkeys={}
    for o in env.objects:
        k=(o.assets_file.name,o.path_id)
        if k in objects:
            newraw=patch(o.get_raw_data(),objects[k]);o.set_raw_data(newraw);editedkeys[k]=objects[k]
    assert len(editedkeys)==len(objects)
    dest=out/'patch'/rel;dest.parent.mkdir(parents=True,exist_ok=True)
    result=env.file.save(packer='lz4') if src.suffix!='.assets' and not src.name.startswith('level') else env.file.save()
    dest.write_bytes(result)
    reload=UnityPy.load(str(dest));dst={(o.assets_file.name,o.path_id):o.get_raw_data() for o in reload.objects}
    assert orig_objects.keys()==dst.keys()
    for k,raw in orig_objects.items():
        if k in editedkeys:verify_strings(dst[k],editedkeys[k])
        else:assert raw==dst[k],k
    if rel.name=='resources.assets':
        for object_id,kind in [(1591,'eula'),(1594,'credits')]:
            old=read_table(orig_objects[next(k for k in orig_objects if k[1]==object_id)],kind)
            new=read_table(dst[next(k for k in dst if k[1]==object_id)],kind)
            expected={r['id']:r['pt'] for r in extra if r['object']==object_id}
            assert len(old)==len(new)
            for a,b in zip(old,new):assert b['en']==expected[a['id']]
    entry={'path':str(rel).replace('\\','/'),'source_sha256':sha(source_bytes),'patch_sha256':sha(result),'modified_objects':len(objects),'changed_fields':sum(len(v) for v in objects.values()),'unchanged_objects':len(orig_objects)-len(editedkeys),'accepted_upgrade_hashes':[v01['patch_sha256']] if rel.name=='resources.assets' else []}
    manifest['files'].append(entry)
    print(entry['path'],entry['changed_fields'],'fields verified',flush=True)
(out/'manifesto.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
with (out/'textos.tsv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.writer(f,delimiter='\t');w.writerow(['ID','Inglês','Português brasileiro'])
    w.writerows((r['id'],r['en'],r['pt']) for r in rows+extra)
with (out/'textos-interface.tsv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.writer(f,delimiter='\t');w.writerow(['Arquivo','Objeto','Inglês','Português brasileiro'])
    w.writerows((pathlib.Path(r['file']).name,r['object'],r['text'],r['pt']) for r in static)
print('DONE',len(manifest['files']),'files',flush=True)
