import json, pathlib, re, collections
base = pathlib.Path(__file__).parent
rows = json.loads((base/'translated_v02.json').read_text(encoding='utf8'))
by_id = {r['id']: r for r in rows}
overrides = {}
tokens = re.compile(r'<[^<>]*>|\{[^{}]*\}|\$s')
for line in (base/'review_v03.txt').read_text(encoding='utf8').splitlines():
    if not line.strip() or line.startswith('#'): continue
    key, value = line.split('|', 1)
    assert key not in overrides, key
    assert key in by_id, key
    value = value.replace('\\n', '\n')
    assert tokens.findall(by_id[key]['en']) == tokens.findall(value), (key, tokens.findall(by_id[key]['en']), tokens.findall(value))
    overrides[key] = value
# Repeat identical dialogue consistently. Limit propagation to the same ID family.
reviewed_en = {(key.split('_')[0],by_id[key]['en']): value for key,value in overrides.items()}
siblings = {'Older Brother':'irmão mais velho','Younger Brother':'irmão mais novo',
    'Older Sister':'irmã mais velha','Younger Sister':'irmã mais nova',
    'Older Brothers':'irmãos mais velhos','Younger Brothers':'irmãos mais novos'}
twin_names = {'Rooster':'Galo','Daruma':'Daruma','Boots':'Botas','Bat':'Taco',
    'Pumpkin':'Abóbora','Trophy':'Troféu','Hour Hand':'Ponteiro das horas',
    'Minute Hand':'Ponteiro dos minutos','Clown':'Palhaço','Little Ma':'Pequena Ma',
    'Ball':'Bola','Super Ball':'Superbola','Penguin':'Pinguim','Lefty':'Canhota','Righty':'Destra',
    'Crab':'Caranguejo','Beachball':'Bola de praia','Binoculars':'Binóculos','Sheep':'Ovelha',
    'Champ':'Campeão','Co-champ':'Cocampeão','Papa Bear':'Papai Urso','Baby Bear':'Bebê Urso',
    'Plant':'Planta','Lantern':'Lanterna','Pine':'Pinho','Apple':'Maçã','Watermelon':'Melancia',
    'Peach':'Pêssego','Pin':'Pino','Bonsai':'Bonsai','Principal':'Diretor','Business Owner':'Comerciante',
    'Celebrity':'Celebridade','Merman':'Tritão','Cow':'Vaca','Lion':'Leão','Dancer':'Dançarino',
    'Hat':'Chapéu','Vending Machine':'Máquina de vendas','Paper Bag':'Saco de papel','Mailbox':'Caixa de correio',
    'Snowman':'Boneco de neve','Grand Opening':'Inauguração','Manhole Cover':'Tampa de bueiro',
    'Killer Movers':'Mudanças Mortais','Turtle':'Tartaruga','Senior Sr.':'Senhor Sênior','Junior Jr.':'Júnior Jr.'}
for r in rows:
    if r['id'].startswith('OT_CON_') and ' (' in r['en']:
        name, relation = r['en'].split(' (',1)
        relation = relation.rstrip(')')
        if relation in siblings:
            overrides[r['id']] = twin_names.get(name,name) + ' (' + siblings[relation] + ')'
changed = []
for r in rows:
    pt = overrides.get(r['id'], reviewed_en.get((r['id'].split('_')[0],r['en']),r['pt']))
    if pt != r['pt']:
        changed.append({'id':r['id'],'en':r['en'],'before':r['pt'],'after':pt})
        r['pt'] = pt
        r['method'] = 'context-reviewed-v03'
    assert tokens.findall(r['en']) == tokens.findall(r['pt']),r['id']
(base/'translated_v03.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf8')
(base/'review_changes_v03.json').write_text(json.dumps(changed,ensure_ascii=False,indent=2),encoding='utf8')
print('Explicit reviews:',len(overrides),'Changed entries incl. repeated dialogue:',len(changed))
print('Changes by group:',dict(collections.Counter(x['id'].split('_')[0] for x in changed)))
