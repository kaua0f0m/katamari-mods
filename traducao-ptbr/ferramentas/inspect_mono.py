exec(open(__file__.replace('inspect_mono.py','inspect_assets.py'),encoding='utf-8').read().split("print(collections.Counter")[0])
records=[]
def strings(x,path=''):
    if isinstance(x,str) and x: yield path,x
    elif isinstance(x,dict):
        for k,v in x.items(): yield from strings(v,path+'/'+k)
    elif isinstance(x,list):
        for i,v in enumerate(x): yield from strings(v,path+'/'+str(i))
for o in env.objects:
    if o.type.name=='MonoBehaviour':
        try:
            d=o.parse_as_dict()
            ss=list(strings(d))
            records.append({'id':o.path_id,'name':d.get('m_Name'),'script':d.get('m_Script'),'keys':list(d),'strings':ss})
            if len(ss)>3 or any(re.search(r'\s[a-zA-Z]+\s',s) for _,s in ss): print(o.path_id,d.get('m_Name'),list(d),len(ss),repr(ss[:8]))
            (out/f'mono_{o.path_id}.json').write_text(json.dumps(d,ensure_ascii=True,indent=2),encoding='utf-8')
        except Exception as e: print('ERROR',o.path_id,str(e)[:100])
(base/'mono_index.json').write_text(json.dumps(records,ensure_ascii=True,indent=2),encoding='utf-8')
