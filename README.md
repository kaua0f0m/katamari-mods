# Katamari Damacy REROLL — mods para PC (Steam)

Código-fonte, ferramentas e textos de três mods não oficiais para a versão Steam de **Katamari Damacy REROLL**:

| Pasta | Versão | O que é |
|---|---|---|
| [`traducao-ptbr/`](traducao-ptbr/) | 0.3 | Tradução para português brasileiro dos textos do jogo |
| [`katamari-plus/`](katamari-plus/) | 0.2 | Tempo infinito configurável, encerrar fase pela pausa, jukebox, salvamento/carregamento automático com escolha de slot, pular abertura |
| [`extramod/`](extramod/) | 0.1 | Versão anterior das melhorias de jogabilidade (substituída pelo Katamari Plus) |

Cada pasta tem o seu `LEIA-ME` com detalhes, o instalador (`Instalar.ps1`), o desinstalador e o `manifesto.json` com os hashes SHA-256 dos arquivos esperados.

## Arquivos do jogo não incluídos

Este repositório **não contém arquivos do jogo**, nem originais nem modificados (`Assembly-CSharp.dll`, `resources.assets`, `level*`, `gamemain`, `ui_pause` etc.). Eles pertencem à Bandai Namco. Por isso a pasta `patch/` que os instaladores esperam não está aqui. Os instaladores só funcionam com os pacotes gerados a partir de uma cópia própria do jogo.

## Tradução PT-BR

- `textos.tsv` e `textos-interface.tsv`: texto original e tradução, lado a lado.
- `revisao-v03.json`: as 1.081 alterações da revisão 0.3 em relação à 0.2.
- `ferramentas/`: scripts Python usados para extrair os textos (UnityPy), traduzir (Argos Translate), aplicar revisões e reconstruir os assets, além das tabelas revisadas e caches de tradução.

Os modelos do Argos Translate (`en_pt`, `en_pb`) e as dependências Python não estão incluídos; instale-os à parte.

## Katamari Plus / ExtraMod

O código em `source/` é compilado contra as DLLs do próprio jogo e aplicado com Mono.Cecil (`PatchGame.cs`). Para recompilar, coloque as DLLs originais do jogo em `Managed/` e o Mono.Cecil 0.11.6 em `tools/cecil/lib/net40/`, como indica o `Build.ps1`.

## Aviso

Projeto de fã, sem vínculo com a Bandai Namco. Use por sua conta e risco e mantenha backup dos seus saves.
