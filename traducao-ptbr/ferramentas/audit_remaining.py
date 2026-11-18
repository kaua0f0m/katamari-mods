import pathlib,sys,struct,json,re,collections
base=pathlib.Path(__file__).parent
sys.path.insert(0,str(base/'pydeps'))
import UnityPy
game=pathlib.Path(r'D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL\katamari_Data')
globenv=UnityPy.load(str(base/'source_extra'/'globalgamemanagers.assets'))
global_scripts={o.path_id:o.parse_as_object().m_ClassName for o in globenv.objects if o.type.name=='MonoScript'}
def scan_strings(raw):
    for pos in range(28,len(raw)-4,4):
        n=struct.unpack_from('<i',raw,pos)[0]
        if not 1<=n<=20000 or pos+4+n>len(raw):continue
        try:s=raw[pos+4:pos+4+n].decode('utf8')
        except UnicodeError:continue
        if any(ord(c)<32 and c not in '\n\t\r' for c in s):continue
        if re.search('[A-Za-z]',s):yield pos,s
records=[];summ=[]
paths=[base/'source'/'resources.assets']+list((base/'source_extra').iterdir())
paths+= [p for p in (game/'StreamingAssets'/'StandaloneWindows').iterdir() if p.is_file() and not p.suffix]
for path in paths:
    if path.name=='globalgamemanagers.assets':continue
    try:env=UnityPy.load(str(path))
    except Exception as ex:print('ERROR',str(path),str(ex),flush=True);continue
    local_scripts={(id(o.assets_file),o.path_id):o.parse_as_object().m_ClassName for o in env.objects if o.type.name=='MonoScript'}
    rows=[];localized=set()
    for o in env.objects:
        if o.type.name!='MonoBehaviour':continue
        raw=o.get_raw_data()
        if len(raw)<28:continue
        fid,pid=struct.unpack_from('<iq',raw,16)
        cl=''
        if fid==0:cl=local_scripts.get((id(o.assets_file),pid),'')
        elif fid-1<len(o.assets_file.externals) and o.assets_file.externals[fid-1].path.endswith('globalgamemanagers.assets'):cl=global_scripts.get(pid,'')
        else:
            try:
                from UnityPy.classes import PPtr
                cl=PPtr(m_FileID=fid,m_PathID=pid,assetsfile=o.assets_file).deref_parse_as_object().m_ClassName
            except Exception:pass
        go=struct.unpack_from('<iq',raw,0)
        gokey=(id(o.assets_file),go)
        if cl in ['UITextLocalizer','UITextLocalizerPlatform','UIControllerTextLocalizer','PadTypeText']:localized.add(gokey)
        if cl in ['Text','TextMeshProUGUI','TextMeshPro','LanguageData']:
            ss=list(scan_strings(raw))
            if cl=='Text':ss=[(p,s) for p,s in ss if p>=200 and not s.startswith('UnityEngine.')]
            for p,s in ss:
                rows.append({'file':str(path),'asset':o.assets_file.name,'object':o.path_id,'offset':p,'class':cl,'text':s,'gokey':gokey})
    for r in rows:
        r['localized']=r.pop('gokey') in localized
        records.append(r)
    summ.append({'file':str(path),'objects':len(env.objects),'texts':len(rows)})
    if rows:print(path.name,'text fields',len(rows),'unlocalized',sum(not r['localized'] for r in rows),flush=True)
(base/'remaining_audit.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf8')
(base/'remaining_audit_files.json').write_text(json.dumps(summ,indent=2),encoding='utf8')
print('DONE',len(paths),'files',len(records),'fields',flush=True)
