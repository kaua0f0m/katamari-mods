import sys,pathlib,json,re
base=pathlib.Path(__file__).parent
sys.path.insert(0,str(base/'translate_ready'))
import ctranslate2,sentencepiece
pkg=base/'model_br'/'translate-en_pb-1_9'
sp=sentencepiece.SentencePieceProcessor(model_file=str(pkg/'sentencepiece.model'))
tr=ctranslate2.Translator(str(pkg/'model'),device='cpu',compute_type='int8',inter_threads=2,intra_threads=4)
cachepath=base/'extra_translation_cache.json'
cache=json.loads(cachepath.read_text(encoding='utf8'))
keys=[k for k in cache if k.isupper()]
for a in range(0,len(keys),16):
    ks=keys[a:a+16]
    rs=tr.translate_batch([sp.encode(k.lower(),out_type=str) for k in ks],beam_size=4,max_decoding_length=512)
    for k,r in zip(ks,rs):cache[k]=''.join(r.hypotheses[0]).replace('▁',' ').strip().upper()
print('Refined uppercase segments',len(keys),flush=True)
cachepath.write_text(json.dumps(cache,ensure_ascii=False,indent=2),encoding='utf8')
