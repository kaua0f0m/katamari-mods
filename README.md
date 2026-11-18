# Katamari Damacy REROLL — mods para PC (Steam)

Código-fonte, ferramentas e textos de dois mods não oficiais para a versão Steam de **Katamari Damacy REROLL**:

| Pasta | Versão | O que é |
|---|---|---|
| [`traducao-ptbr/`](traducao-ptbr/) | 0.3 | Tradução para português brasileiro dos textos do jogo |
| [`katamari-plus/`](katamari-plus/) | 0.2 | Tempo infinito configurável, encerrar fase pela pausa, jukebox, salvamento/carregamento automático com escolha de slot, pular abertura |

Cada pasta tem o seu `LEIA-ME` com detalhes, o instalador (`Instalar.ps1`), o desinstalador e o `manifesto.json` com os hashes SHA-256 dos arquivos esperados.

## Download

Os pacotes prontos para instalar estão em [Releases](https://github.com/kaua0f0m/katamari-mods/releases):

- [Tradução PT-BR 0.3](https://github.com/kaua0f0m/katamari-mods/releases/tag/traducao-v0.3)
- [Katamari Plus 0.2](https://github.com/kaua0f0m/katamari-mods/releases/tag/plus-v0.2)

Baixe o ZIP, extraia, feche o jogo e execute `Instalar.ps1` no PowerShell. Se o jogo não estiver em `D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL`, informe o caminho: `.\Instalar.ps1 -GameDir 'C:\caminho\do\jogo'`.

O código e os textos ficam no repositório; os arquivos modificados do jogo (pasta `patch/`) só vão nos ZIPs das releases.

## Tradução PT-BR

- `textos.tsv` e `textos-interface.tsv`: texto original e tradução, lado a lado.
- `revisao-v03.json`: as 1.081 alterações da revisão 0.3 em relação à 0.2.
- `ferramentas/`: scripts Python usados para extrair os textos (UnityPy), traduzir (Argos Translate), aplicar revisões e reconstruir os assets, além das tabelas revisadas e caches de tradução.

Os modelos do Argos Translate (`en_pt`, `en_pb`) e as dependências Python não estão incluídos; instale-os à parte.

## Katamari Plus

O código em `source/` é compilado contra as DLLs do próprio jogo e aplicado com Mono.Cecil (`PatchGame.cs`). Para recompilar, coloque as DLLs originais do jogo em `Managed/` e o Mono.Cecil 0.11.6 em `tools/cecil/lib/net40/`, como indica o `Build.ps1`.

## Aviso

Projeto de fã, sem vínculo com a Bandai Namco. Use por sua conta e risco e mantenha backup dos seus saves.
