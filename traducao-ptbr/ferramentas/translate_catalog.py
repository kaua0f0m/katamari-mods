import sys,pathlib,json,re,time
base=pathlib.Path(__file__).parent
sys.path.insert(0,str(base/'translate_ready'))
import ctranslate2,sentencepiece
pkg=base/'model_br'/'translate-en_pb-1_9'
sp=sentencepiece.SentencePieceProcessor(model_file=str(pkg/'sentencepiece.model'))
tr=ctranslate2.Translator(str(pkg/'model'),device='cpu',compute_type='int8',inter_threads=2,intra_threads=4)
rows=json.loads((base/'catalog.json').read_text(encoding='utf8'))
reviewed={}
for line in (base/'reviewed.tsv').read_text(encoding='utf8').splitlines():
    if line.strip():
        k,v=line.split('\t',1); reviewed[k]=v.replace('\\n','\n')
for line in (base/'names_reviewed.tsv').read_text(encoding='utf8').splitlines():
    if line.strip():
        k,v=line.split('\t',1);reviewed['OT_OBJ_'+k]=v
countries='''Estados Unidos da América|Antígua e Barbuda|El Salvador|Canadá|Cuba|Guatemala|Granada|Costa Rica|Jamaica|São Cristóvão e Névis|São Vicente e Granadinas|Santa Lúcia|Dominica|República Dominicana|Trinidad e Tobago|Nicarágua|Haiti|Panamá|Bahamas|Barbados|Belize|Honduras|México|Argentina|Uruguai|Equador|Gana|Colômbia|Suriname|Chile|Paraguai|Brasil|Venezuela|Peru|Bolívia|Austrália|Kiribati|Samoa|Ilhas Salomão|Tuvalu|Tonga|Nauru|Nova Zelândia|Vanuatu|Papua-Nova Guiné|Palau|Fiji|Ilhas Marshall|Estados Federados da Micronésia|Afeganistão|Emirados Árabes Unidos|Iêmen|Israel|Iraque|Irã|Índia|Indonésia|Omã|Catar|Camboja|Chipre|Kuwait|Arábia Saudita|Síria|Singapura|Sri Lanka|Tailândia|Turquia|Nepal|Bahrein|Paquistão|Bangladesh|Filipinas|Butão|Brunei|Vietnã|Malásia|Mianmar|Maldivas|Mongólia|Jordânia|Laos|Líbano|Coreia|China|Japão|Coreia do Norte|Timor-Leste|Argélia|Angola|Uganda|Egito|Etiópia|Eritreia|Gana|Cabo Verde|Gabão|Camarões|Gâmbia|Guiné|Guiné-Bissau|Quênia|Costa do Marfim|Comores|Congo|República Democrática do Congo|São Tomé e Príncipe|Zâmbia|Serra Leoa|Djibuti|Zimbábue|Sudão|Suazilândia|Seicheles|Guiné Equatorial|Senegal|Somália|Tanzânia|Chade|África|Tunísia|Togo|Nigéria|Namíbia|Níger|Burkina Faso|Burundi|Benim|Botsuana|Madagascar|Malawi|Mali|África do Sul|Moçambique|Maurício|Mauritânia|Marrocos|Líbia|Libéria|Ruanda|Lesoto|Islândia|Irlanda|Albânia|Andorra|Inglaterra|Itália|Estônia|Áustria|Holanda|Grécia|Croácia|San Marino|Suíça|Suécia|Espanha|Eslováquia|Eslovênia|República Tcheca|Dinamarca|Alemanha|Noruega|Vaticano|Hungria|Finlândia|França|Bulgária|Bélgica|Polônia|Portugal|Malta|Mônaco|Letônia|Lituânia|Liechtenstein|Luxemburgo|Romênia|Macedônia|Sérvia e Montenegro|Bósnia e Herzegovina|Azerbaijão|Armênia|Ucrânia|Uzbequistão|Cazaquistão|Quirguistão|Geórgia|Tadjiquistão|Turcomenistão|Belarus|Moldávia|Rússia|Groenlândia|Antártida|Taiwan'''.split('|')
assert len(countries)==195,len(countries)
for i,s in enumerate(countries,1501):reviewed[f'OT_OBJ_{i:04d}']=s
for row in rows:
    if row['id'].startswith('OT_PRS_') and row['id'] not in reviewed:reviewed[row['id']]=row['en']
    if row['id'].startswith('OT_OBJ_'):
        i=int(row['id'][-4:])
        if 1474<=i<=1476 or 1478<=i<=1500 or 1696<=i<=1718:reviewed[row['id']]=row['en']
        match=re.fullmatch(r'(.+) Residence\s*',row['en'])
        if match:reviewed[row['id']]='Casa de '+match[1]
shared=json.loads((base/'shared_reviewed.json').read_text(encoding='utf8'))
objectnames={r['en'].strip():reviewed[r['id']] for r in rows if r['id'].startswith('OT_OBJ_') and r['id'] in reviewed}
for row in rows:
    normalized=re.sub(r'\s+',' ',row['en']).strip()
    if normalized in shared:reviewed[row['id']]=shared[normalized]
    elif row['id'].startswith(('OT_CON_','OT_KNG_')) and row['en'].strip() in objectnames:reviewed[row['id']]=objectnames[row['en'].strip()]
for row in rows:
    if row['id'].startswith('UI_SYS_'):
        n=int(row['id'][-3:])
        if n in {12,13,14,39,40,74,75,126,127,128,175,194,195,196,198,199,200,201,202,203,204,205} or 47<=n<=55 or 225<=n<=238 or 250<=n<=255:reviewed[row['id']]=row['en']
fixed={
 'We':'Nós','Our':'Nosso','Us':'Nós','Prince':'Príncipe','The Prince':'O Príncipe',
 'King':'Rei','The King':'O Rei','Queen':'Rainha','King of All Cosmos':'Rei de Todo o Cosmo',
 'The King of All Cosmos':'O Rei de Todo o Cosmo','Earth':'Terra','EARTH':'TERRA',
 'HOME PLANET':'PLANETA NATAL','Home Planet':'Planeta Natal','Moon':'Lua','The Moon':'A Lua',
 'Cancer':'Câncer','Cygnus':'Cisne','Corona Borealis':'Coroa Boreal','Pisces':'Peixes',
 'Virgo':'Virgem','Ursa Major':'Ursa Maior','Gemini':'Gêmeos','Taurus':'Touro','North Star':'Estrela Polar',
 'katamari':'katamari','Katamari':'Katamari',"Charge'n'Roll":'Arrancada','Quick Turn':'Meia-volta',
 'ON':'LIGADO','OFF':'DESLIGADO','Yes':'Sim','No':'Não','OK':'OK','Look':'Olhar','Jump':'Pular',
 'rolling':'rolar','roll':'rolar','Roll':'Rolar','cosmos':'cosmo','Cosmos':'Cosmo',
 'Juice Box':'Caixinha de suco','Strawberry':'Morango','Caramel':'Caramelo','Thumbtack':'Tachinha',
}
tags=re.compile(r'(\{[^{}]*\}|<[^<>]*>|\$s|(?<=[.!?])\s+(?=[A-Z]))')
def preserve(row):
    k=row['id']; s=row['en']
    return (k.startswith('UI_MSH_') and 23<=int(k.rsplit('_',1)[1])<=45) or (k.startswith('UI_MON_') and 22<=int(k.rsplit('_',1)[1])<=41) or not re.search('[A-Za-z]',s) or s.startswith('Katamari Damacy')
def parts(s): return tags.split(s)
def is_text(s):return bool(s.strip()) and not re.fullmatch(r'\{[^{}]*\}|<[^<>]*>|\$s|\s+',s)
def key(s):return s.strip()
cachepath=base/'translation_cache.json'
cache=json.loads(cachepath.read_text(encoding='utf8')) if cachepath.exists() else {}
cache.update(fixed)
need={}
for row in rows:
    if row['id'] in reviewed or preserve(row):continue
    for s in parts(row['en']):
        t=key(s)
        if is_text(s) and re.search('[A-Za-z]',t) and t not in cache:
            normalized=re.sub(r'\s+',' ',t)
            need[t]=normalized.lower() if row['id'].startswith(('OT_OBJ_','OT_CTG_')) else normalized
pending=list(need)
print('Segments to translate',len(pending),flush=True)
start=time.time()
for a in range(0,len(pending),64):
    ks=pending[a:a+64]
    rs=tr.translate_batch([sp.encode(need[k],out_type=str) for k in ks],beam_size=4,max_decoding_length=220,max_batch_size=32)
    for k,r in zip(ks,rs):
        t=''.join(r.hypotheses[0]).replace('▁',' ').strip()
        t=re.sub(r'\s+',' ',t)
        t=re.sub(r'\b[Kk]atamari\b','katamari',t)
        t=t.replace('Rei de Todos os Cosmos','Rei de Todo o Cosmo').replace('Rei de Todo o Cosmos','Rei de Todo o Cosmo')
        if not k.endswith(('.', '!', '?')):t=t.rstrip('.')
        if k.isupper():t=t.upper()
        elif k[:1].isupper():t=t[:1].upper()+t[1:]
        cache[k]=t or k
    cachepath.write_text(json.dumps(cache,ensure_ascii=False,indent=2),encoding='utf8')
    print(f'{min(a+64,len(pending))}/{len(pending)} segments; {time.time()-start:.0f}s',flush=True)
translated=[]
def wrap_like_source(text,source):
    if '\n' not in source:return text
    def visible(s):return len(re.sub(r'\{[^{}]*\}|<[^<>]*>','',s))
    width=min(52,max(35,max(visible(line) for line in source.splitlines())+3))
    lines=[];line=''
    for word in text.split():
        joined=line+' '+word if line else word
        if line and visible(joined)>width:lines.append(line);line=word
        else:line=joined
    if line:lines.append(line)
    return '\n'.join(lines)
for row in rows:
    if row['id'] in reviewed:t=reviewed[row['id']];method='reviewed'
    elif preserve(row):t=row['en'];method='preserved'
    else:
        out=[]
        for s in parts(row['en']):
            k=key(s)
            if is_text(s) and k in cache:
                lead=s[:len(s)-len(s.lstrip())];tail=s[len(s.rstrip()):]
                out.append(lead+cache[k]+tail)
            else:out.append(s)
        t=wrap_like_source(''.join(out),row['en']);method='automatic'
    assert re.findall(r'\{[^{}]*\}|<[^<>]*>',t)==re.findall(r'\{[^{}]*\}|<[^<>]*>',row['en']),row['id']
    translated.append({**row,'pt':t,'method':method})
(base/'translated.json').write_text(json.dumps(translated,ensure_ascii=False,indent=2),encoding='utf8')
print('DONE',len(translated),flush=True)
