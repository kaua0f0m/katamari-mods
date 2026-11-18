import sys,pathlib,struct,json,re
sys.path.insert(0,str(pathlib.Path(__file__).parent/'pydeps'))
import UnityPy,dnfile
base=pathlib.Path(__file__).parent
env=UnityPy.load(str(base/'source'/'resources.assets'))
records=[]
for o in env.objects:
    if o.type.name!='MonoBehaviour': continue
    raw=o.get_raw_data()
    strs=[]
    for i in range(0,len(raw)-4,4):
        n=struct.unpack_from('<i',raw,i)[0]
        if not 1<=n<=10000 or i+4+n>len(raw): continue
        try: s=raw[i+4:i+4+n].decode('utf8')
        except UnicodeError: continue
        if any(ord(c)<32 and c not in '\r\n\t' for c in s): continue
        if not any(c.isalpha() for c in s): continue
        strs.append((i,s))
    records.append({'id':o.path_id,'size':len(raw),'strings':strs})
    if strs: print(o.path_id,len(raw),len(strs),ascii(strs[:5]))
    (base/'extracted'/f'raw_{o.path_id}.bin').write_bytes(raw)
(base/'raw_index.json').write_text(json.dumps(records,ensure_ascii=True,indent=2),encoding='utf8')
pe=dnfile.dnPE(str(base/'source'/'Assembly-CSharp.dll'))
for t in pe.net.mdtables.TypeDef:
    name=str(t.TypeName)
    if re.search('text|lang|local|message|word|mono',name,re.I):
        print('TYPE',name, 'FIELDS', [str(f.row.Name) for f in t.FieldList], 'METHODS',[str(m.row.Name) for m in t.MethodList])
