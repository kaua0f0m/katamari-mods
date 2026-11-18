import sys,pathlib,zipfile,json
base=pathlib.Path(__file__).parent
sys.path.insert(0,str(base/'translate_ready'))
import ctranslate2,sentencepiece
modeldir=base/'model_br'
if not modeldir.exists():
    with zipfile.ZipFile(base/'en_pb.argosmodel') as z: z.extractall(modeldir)
pkg=next(modeldir.iterdir())
print('Model',pkg,flush=True)
print([str(p.relative_to(pkg)) for p in pkg.rglob('*')][:30],flush=True)
sp=sentencepiece.SentencePieceProcessor(model_file=str(pkg/'sentencepiece.model'))
tr=ctranslate2.Translator(str(pkg/'model'),device='cpu',compute_type='int8',inter_threads=2,intra_threads=4)
texts=['Ah, hello, Prince. This is the King. Long time no see.',
"We thought today would be a good day for {#7}rolling{#0}. The katamari.{^5} The Prince.",
"We thought today would be a good day for ZXQ0QXZrollingZXQ1QXZ. The katamari.ZXQ2QXZ The Prince.",
'Load {0} file. Is this OK?', 'This katamari is a little too small.', 'Whoever it is, please make my cat a star.',
'Juice Box','Roll up as many crabs as you can.','Make Cancer', 'Dad, I feel the cosmos!', "Charge'n'Roll",
"We thought today would be a good day for ZXQ0QXZ rolling ZXQ1QXZ. The katamari. ZXQ2QXZ The Prince.",
"We thought today would be a good day for {0} rolling {1}. The katamari. {2} The Prince.",
"For that purpose alone, We went to the bother, {0} the bother of making this space."]
results=tr.translate_batch([sp.encode(t,out_type=str) for t in texts],beam_size=4,max_decoding_length=256)
for a,b in zip(texts,results): print(a,'=>',sp.decode(b.hypotheses[0]).replace('▁',' '), 'TOKENS', b.hypotheses[0],flush=True)
