import sys,pathlib,json,re,time,shutil
base=pathlib.Path(__file__).parent
sys.path.insert(0,str(base/'translate_ready'))
import ctranslate2,sentencepiece
pkg=base/'model_br'/'translate-en_pb-1_9'
sp=sentencepiece.SentencePieceProcessor(model_file=str(pkg/'sentencepiece.model'))
tr=ctranslate2.Translator(str(pkg/'model'),device='cpu',compute_type='int8',inter_threads=2,intra_threads=4)
orig=base/'translated_v01.json'
if not orig.exists():shutil.copyfile(base/'translated.json',orig)
main=json.loads(orig.read_text(encoding='utf8'))
overrides={}
for line in (base/'remaining_reviewed.tsv').read_text(encoding='utf8').splitlines():
    k,v=line.split('\t',1);overrides[k]=v.replace('\\n','\n')
for r in main:
    if r['id'] in overrides:r['pt']=overrides[r['id']];r['method']='reviewed'
(base/'translated_v02.json').write_text(json.dumps(main,ensure_ascii=False,indent=2),encoding='utf8')
extras=json.loads((base/'extra_catalog.json').read_text(encoding='utf8'))
audit=json.loads((base/'remaining_audit.json').read_text(encoding='utf8'))
roles=json.loads((base/'credit_translations.json').read_text(encoding='utf8'))
roles.update({'Producer':'Produção','Executive Producer':'Produção executiva','President & CEO':'Presidente e diretor executivo','President & COO':'Presidente e diretor de operações','Development':'Desenvolvimento','QUALITY ASSURANCE':'GARANTIA DE QUALIDADE','MARKETING & SALES':'MARKETING E VENDAS','OPERATIONS':'OPERAÇÕES','LOCAL MARKETING TEAMS':'EQUIPES LOCAIS DE MARKETING','UK':'REINO UNIDO','FRANCE / BENELUX':'FRANÇA / BENELUX','DEUTSCHLAND':'ALEMANHA','IBERICA':'PENÍNSULA IBÉRICA','ITALY':'ITÁLIA','NORDIC':'PAÍSES NÓRDICOS','HELLAS':'GRÉCIA','EASTERN EUROPE':'EUROPA ORIENTAL','AUSTRALIA':'AUSTRÁLIA','SPECIAL THANKS TO':'AGRADECIMENTOS ESPECIAIS','Produced by BANDAI NAMCO Entertainment Inc.':'Produzido por BANDAI NAMCO Entertainment Inc.','Produced by 株式会社バンダイナムコエンターテインメント':'Produzido por BANDAI NAMCO Entertainment Inc.'})
credit_texts=[r['en'] for r in extras if r['object']==1594]
credit_texts.extend(r['text'] for r in audit if len(r['text'])>500 and '<color="#FBFF70">' in r['text'])
for text in credit_texts:
    for h in re.findall(r'<color="#FBFF70">([^<>]+)</color>',text):
        k=h.strip()
        if re.search('[A-Za-z]',k) and k not in roles:roles[k]=None
# Keep companies and people's names; translate only headings and role labels.
needed=[k for k,v in roles.items() if v is None]
for a in range(0,len(needed),32):
    ks=needed[a:a+32]
    rs=tr.translate_batch([sp.encode(k.lower(),out_type=str) for k in ks],beam_size=4,max_decoding_length=160)
    for k,r in zip(ks,rs):
        t=''.join(r.hypotheses[0]).replace('▁',' ').strip().rstrip('.')
        roles[k]=t[:1].upper()+t[1:]
(base/'all_credit_roles.json').write_text(json.dumps(roles,ensure_ascii=False,indent=2),encoding='utf8')
songmap={r['en']:r['pt'] for r in main if r['id'].startswith('UI_MON_')}
songmap.update({'The Last Samba':'O último samba','Walking on a Star!':'Caminhando em uma estrela!','Katamari On The Rocks':'Katamari com gelo','Katamari Mambo (Katamari Syndrome Re-mix)':'Mambo do Katamari (Remix da síndrome Katamari)'})
def credits(text):
    def sub(m):
        x=m[2];k=x.strip();v=roles.get(k)
        return m[1]+(x[:len(x)-len(x.lstrip())]+v+x[len(x.rstrip()):] if v is not None else x)+m[3]
    text=re.sub(r'(<(?:color|size)=[^>]*>)([^<>]+)(</(?:color|size)>)',sub,text)
    for a,b in songmap.items():text=text.replace('「'+a+'」','「'+b+'」')
    for a,b in roles.items():
        if a=='(Courtesy of FOR LIFE MUSIC ENTERTAINMENT, INC)' or a=='NHI IS Team':text=text.replace(a,b)
    return text
splitter=re.compile(r'(\r?\n|(?<=[.!?])\s+(?=[A-Z(]))')
url_re=re.compile(r'(https?://[^\s)]+|www\.[^\s)]+|[\w.+-]+@[\w.-]+\.[A-Za-z]+)')
def pieces(text):
    parts=[]
    for part in splitter.split(text):
        if len(part)>900:
            for c in re.split(r'(?<=[;,]) ',part):parts.extend(url_re.split(c));parts.append(' ')
            if parts and parts[-1]==' ':parts.pop()
        else:parts.extend(url_re.split(part))
    return parts
cachepath=base/'extra_translation_cache.json'
cache=json.loads(cachepath.read_text(encoding='utf8')) if cachepath.exists() else {}
needed={s.strip() for r in extras if r['object']==1591 for s in pieces(r['en']) if s.strip() and re.search('[A-Za-z]',s) and not url_re.fullmatch(s)}-cache.keys()
needed=sorted(needed)
print('Contract segments',len(needed),flush=True)
for a in range(0,len(needed),32):
    ks=needed[a:a+32]
    rs=tr.translate_batch([sp.encode(k,out_type=str) for k in ks],beam_size=4,max_decoding_length=512,max_batch_size=16)
    for k,r in zip(ks,rs):
        t=''.join(r.hypotheses[0]).replace('▁',' ').strip()
        if k.isupper():t=t.upper()
        if not k.endswith(('.', '!', '?')):t=t.rstrip('.')
        cache[k]=t
    cachepath.write_text(json.dumps(cache,ensure_ascii=False,indent=2),encoding='utf8')
    print(min(a+32,len(needed)),'/',len(needed),flush=True)
for r in extras:
    if r['object']==1594:r['pt']=credits(r['en'])
    else:
        parts=[]
        for s in pieces(r['en']):
            k=s.strip()
            if k in cache:parts.append(s[:len(s)-len(s.lstrip())]+cache[k]+s[len(s.rstrip()):])
            else:parts.append(s)
        r['pt']=''.join(parts)
    assert re.findall(r'<[^<>]*>|\{[^{}]*\}',r['pt'])==re.findall(r'<[^<>]*>|\{[^{}]*\}',r['en']),r['id']
manual=json.loads((base/'extra_reviewed.json').read_text(encoding='utf8'))
headings={'EULA_0010':'CONDIÇÕES PRÉVIAS DAS LICENÇAS','EULA_0011':'LICENÇA DE USO LIMITADO','EULA_0012':'PROPRIEDADE','EULA_0013':'VIGÊNCIA','EULA_0014':'ALTERAÇÕES','EULA_0015':'MODIFICAÇÃO DO JOGO','EULA_0016':'MOEDA DO JOGO E ITENS VIRTUAIS','EULA_0017':'DIREITO DE ARREPENDIMENTO','EULA_0018':'CONTEÚDO DO USUÁRIO','EULA_0019':'CONDUTA ONLINE','EULA_0020':'POLÍTICA DE DIREITOS AUTORAIS','EULA_0026':'RESOLUÇÃO DE CONTROVÉRSIAS','EULA_0027':'LEI APLICÁVEL','EULA_0028':'FORO','EULA_0029':'TERCEIROS BENEFICIÁRIOS','EULA_0030':'DISPOSIÇÕES GERAIS'}
for r in extras:
    if r['id'] in manual:r['pt']=manual[r['id']]
    elif r['id'] in headings:r['pt']=re.sub(r'(^\s*\d+\.\s*)[^.]+\.',lambda m:m[1]+headings[r['id']]+'.',r['pt'],count=1)
    if r['object']==1591:
        for a,b in [('idade da maioria','maioridade'),('negligência grosseira','negligência grave'),('TORT','ATO ILÍCITO'),('Opt Out Notice','Notificação de Exclusão'),('Class Action Waiver','Renúncia a Ações Coletivas'),('PRIVACY','PRIVACIDADE')]:r['pt']=r['pt'].replace(a,b)
(base/'extra_translated.json').write_text(json.dumps(extras,ensure_ascii=False,indent=2),encoding='utf8')
staticmap={'I Disagree\r':'Discordo\r','I Agree\r':'Concordo\r','Option A':'Opção A','no data':'sem dados','Item Name':'Nome do item','press + button':'aperte o botão +','press start':'aperte Start','OFF':'DESLIGADO','ON':'LIGADO'}
static=[]
for r in audit:
    if r['localized']:continue
    text=r['text']
    if text in staticmap:pt=staticmap[text]
    elif len(text)>500 and '<color="#FBFF70">' in text:pt=credits(text)
    else:continue
    if pt!=text:
        assert re.findall(r'<[^<>]*>',text)==re.findall(r'<[^<>]*>',pt)
        static.append({**r,'pt':pt})
(base/'static_translated.json').write_text(json.dumps(static,ensure_ascii=False,indent=2),encoding='utf8')
print('DONE: additional changed',sum(r['en']!=r['pt'] for r in extras),'static',len(static),'files',sorted(set(pathlib.Path(r['file']).name for r in static)),flush=True)
