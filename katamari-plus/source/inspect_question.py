import sys, pathlib,struct,re
sys.path.insert(0,'work/pydeps')
import UnityPy
p='work/sources_v02/katamari_Data/StreamingAssets/StandaloneWindows/ui_pause'
e=UnityPy.load(p)
for o in e.objects:
 if o.type.name!='GameObject':continue
 d=o.parse_as_object()
 if 'question' not in d.m_Name.lower():continue
 print('GO',o.path_id,d.m_Name)
 for c in d.m_Component:
  q=c.component.deref()
  if q.type.name!='MonoBehaviour':continue
  raw=q.get_raw_data()
  for k in range(0,len(raw)-4,4):
   n=struct.unpack_from('<i',raw,k)[0]
   if 3<=n<=500 and k+4+n<=len(raw):
    try:s=raw[k+4:k+4+n].decode('utf8')
    except:continue
    if all(x.isprintable() or x in '\r\n' for x in s):print(q.path_id,k,repr(s))
