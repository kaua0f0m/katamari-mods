import sys, json, collections, pathlib, re
sys.path.insert(0, str(pathlib.Path(__file__).parent/'pydeps'))
import UnityPy
base=pathlib.Path(__file__).parent
env=UnityPy.load(str(base/'source'/'resources.assets'))
out=base/'extracted'
out.mkdir(exist_ok=True)
print(collections.Counter(o.type.name for o in env.objects))
index=[]
for o in env.objects:
    if o.type.name=='TextAsset':
        d=o.parse_as_object()
        raw=d.m_Script.encode('utf-8','surrogateescape')
        name=re.sub(r'[^\w.\-]','_',d.m_Name)
        (out/f'{o.path_id}_{name}.txt').write_bytes(raw)
        index.append({'id':o.path_id,'name':d.m_Name,'length':len(raw),'preview':repr(raw[:150])})
    elif o.type.name=='MonoScript':
        d=o.parse_as_object()
        if re.search('text|message|lang|local|word', d.m_ClassName, re.I): print('SCRIPT',o.path_id,d.m_ClassName)
(base/'textasset_index.json').write_text(json.dumps(index,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(index[:100],ensure_ascii=True,indent=2))
