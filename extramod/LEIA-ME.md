# Katamari Damacy REROLL — Mod extra 0.1

Para a edição Steam de PC examinada nesta instalação. Compatível com a tradução PT-BR 0.3; os dois pacotes alteram arquivos diferentes.

- Carrega automaticamente o save 1 quando ele existe, pode ser carregado e passa pela verificação de CRC do jogo. Caso contrário, mantém a seleção normal.
- Pula o filme de abertura e a espera na tela de título ao carregar esse save. Cenas da história e dos resultados continuam normais.
- Mantém o tempo nas fases de um jogador. O jogo e a contagem de tempo decorrido continuam funcionando. Tutoriais, demonstrações e modo versus usam as regras originais.
- Com a fase pausada pelo START, SELECT/View abre a confirmação nativa para encerrar a fase. Use os comandos normais do controle para escolher Sim ou Não. No teclado padrão: Tab pausa, V abre a confirmação, Enter confirma e Esc cancela.
- Encerrar uma fase usa a avaliação normal do jogo. Se a meta não foi atingida, o resultado pode ser uma falha. Alguns modos especiais também podem terminar pelas próprias condições originais.
- Salva o progresso no slot 1 a cada 60 segundos, ao voltar ao Planeta Natal e ao receber o fechamento normal do aplicativo. A saída pela Steam tenta acionar esse fechamento. Um encerramento forçado do processo pode impedir a última gravação; nesse caso vale o último salvamento automático.

O save guarda o progresso que o jogo suporta, incluindo fases concluídas e coleção. Não retoma a posição exata do katamari no meio da fase.

A confirmação, as cores, a tipografia e os comandos vêm da pausa do próprio jogo. Foi acrescentada apenas uma indicação de SELECT/View com a fonte existente, sem uma interface externa.

## Instalar e restaurar

Feche o jogo e execute `Instalar.ps1` no PowerShell. Ele verifica a versão e mantém o DLL original ao lado do arquivo, com sufixo `.original-qol-b8e181cf130b`. `Restaurar.ps1` devolve o comportamento original sem mexer na tradução ou no save.

Antes dos testes, uma cópia integral dos saves foi preservada em `work/mod/save-backup-before` no projeto local do Codex. O mod também guarda o par anterior de save 1 e metadados de CRC em `KatamariQoL/previous-save`, na pasta do jogo. Seus registros ficam em `KatamariQoL/qol.log`. Saves pessoais não estão incluídos neste pacote.

## Validação realizada

Compilação contra as bibliotecas Mono do jogo, reabertura do DLL modificado, carregamento automático em execução, salto da abertura, relógio congelado e confirmação de encerramento com controle Xbox. O usuário confirmou o funcionamento da confirmação após a correção do fluxo Sim/Não. O registro de execução confirmou gravações periódicas e ao fechar, com CRC válido. A primeira fase seguiu para a avaliação normal após o encerramento manual.

As demais fases e condições especiais não foram percorridas integralmente. Fontes do helper, patcher e compilação estão em `source/`; para recompilar é preciso fornecer os DLLs originais em `Managed/` e Mono.Cecil 0.11.6 em `tools/cecil/lib/net40/`.
