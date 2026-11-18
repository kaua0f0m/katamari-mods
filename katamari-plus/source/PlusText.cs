using System;
using MyGame;

// Textos do Katamari Plus em todos os idiomas do jogo.
// O idioma segue o do jogo (definido pela Steam). Com a tradução PT-BR instalada,
// o slot de inglês do jogo contém português, então o Plus também usa português.
public sealed partial class KatamariQoL
{
    sealed class PlusLang
    {
        public string Code;
        public string[] Labels;      // 8 linhas do menu
        public string[] Tips;        // 8 descrições
        public string On, Off, Original, Slot, Seconds;
        public string PauseTip, ControlTip, Confirm, Demo;
        public string Continue, Home;  // só usados no português (faixa da pausa)
    }

    const int LangJa = 0, LangEn = 1, LangPt = 6;

    // Índices 0-5 seguem o enum LANGUAGE do jogo (JAPANESE, ENGLISH, FRENCH, GERMAN, ITALIAN, SPANISH).
    static readonly PlusLang[] plusLangs = {
        new PlusLang {
            Code="ja",
            Labels=new[]{"時間無制限","ジュークボックス","オートセーブ","オートロード","オープニングをスキップ","セーブ間隔","ステージを終了","もどる"},
            Tips=new[]{
                "時間が止まったままになります。終了するには、ポーズから Katamari Plus >「ステージを終了」を選んでください。",
                "ステージ中の曲だけを差し替えます。ステージ外で選んだ場合は次のステージから反映されます。「オリジナル」で通常の曲に戻ります。",
                "終了時と定期セーブで進行状況を書き込むスロットを選びます。そのスロットのデータは上書きされます。「オフ」で両方とも無効になります。",
                "次回起動時に読み込むセーブを選びます。空または無効な場合は通常の選択画面になります。「オフ」で手動選択のままになります。",
                "オープニングムービーとタイトル画面の待ち時間をスキップします。ストーリーシーンとリザルトは通常どおりです。",
                "定期セーブの間隔を設定します。星に戻ったときと通常終了したときにもセーブを試みます。",
                "今回の挑戦を終了し、通常の評価に進みます。目標に届いていない場合は失敗になることがあります。",
                "Katamari Plus を閉じて前の画面に戻ります。設定は自動で保存されます。"},
            On="オン",Off="オフ",Original="オリジナル",Slot="セーブ{0}",Seconds="{0}秒",
            PauseTip="Katamari Plus で機能の設定やステージの終了ができます。",
            ControlTip="かんたん操作：左スティックで移動・右スティック左右で回転・右スティック下でダッシュをためる。",
            Confirm="ステージを終了しますか？",Demo="デモの曲"
        },
        new PlusLang {
            Code="en",
            Labels=new[]{"Infinite time","Jukebox","Autosave","Autoload","Skip opening","Autosave interval","End stage","Back"},
            Tips=new[]{
                "Keeps the clock stopped. To end the stage, pause and open Katamari Plus > End stage.",
                "Replaces only the stage music. Outside a stage, the choice applies to the next one. Original restores the normal track.",
                "Choose the slot that receives your progress on exit and on periodic saves. That slot's data will be overwritten. Off disables both.",
                "Choose the save to open on the next launch. If it is empty or invalid, the normal file select appears. Off keeps manual selection.",
                "Skips the opening movie and the wait on the title screen. Story scenes and results play normally.",
                "Sets how often the periodic save runs. The game also tries to save when you return to the planet and when it closes normally.",
                "Ends this attempt and goes to the normal results. If the goal was not reached, the result may be a failure.",
                "Closes Katamari Plus and returns to the previous screen. Options are saved automatically."},
            On="On",Off="Off",Original="Original",Slot="Save {0}",Seconds="{0} s",
            PauseTip="Open Katamari Plus to adjust features or end the stage.",
            ControlTip="Simple controls: left stick moves · right stick sideways turns · right stick down charges the dash.",
            Confirm="End the stage now?",Demo="Demo music"
        },
        new PlusLang {
            Code="fr",
            Labels=new[]{"Temps infini","Juke-box","Sauvegarde auto","Chargement auto","Passer l'intro","Intervalle de sauvegarde","Terminer le niveau","Retour"},
            Tips=new[]{
                "Arrête le chrono. Pour terminer, revenez à la pause et ouvrez Katamari Plus > Terminer le niveau.",
                "Remplace uniquement la musique du niveau. Hors niveau, le choix s'applique au suivant. Original rétablit la piste normale.",
                "Choisissez l'emplacement qui reçoit la progression à la fermeture et lors des sauvegardes périodiques. Ses données seront remplacées. Désactivé coupe les deux.",
                "Choisissez la sauvegarde à ouvrir au prochain lancement. Si elle est vide ou invalide, la sélection normale s'affiche. Désactivé garde la sélection manuelle.",
                "Passe la vidéo d'introduction et l'attente sur l'écran titre. Les scènes de l'histoire et les résultats restent normaux.",
                "Définit la fréquence de la sauvegarde périodique. Le jeu tente aussi de sauvegarder au retour sur la planète et à la fermeture normale.",
                "Met fin à cette tentative et passe à l'évaluation normale. Si l'objectif n'est pas atteint, cela peut compter comme un échec.",
                "Ferme Katamari Plus et revient à l'écran précédent. Les options sont enregistrées automatiquement."},
            On="Activé",Off="Désactivé",Original="Original",Slot="Sauvegarde {0}",Seconds="{0} s",
            PauseTip="Ouvrez Katamari Plus pour régler les fonctions ou terminer le niveau.",
            ControlTip="Contrôles simples : stick gauche pour avancer · stick droit sur les côtés pour tourner · stick droit vers le bas pour charger le sprint.",
            Confirm="Terminer le niveau maintenant ?",Demo="Musique de démo"
        },
        new PlusLang {
            Code="de",
            Labels=new[]{"Unendliche Zeit","Jukebox","Autom. Speichern","Autom. Laden","Intro überspringen","Speicherintervall","Level beenden","Zurück"},
            Tips=new[]{
                "Hält die Uhr an. Zum Beenden pausieren und Katamari Plus > Level beenden öffnen.",
                "Ersetzt nur die Levelmusik. Außerhalb eines Levels gilt die Wahl für das nächste. Original stellt die normale Musik wieder her.",
                "Wähle den Platz, der beim Beenden und beim regelmäßigen Speichern den Fortschritt erhält. Seine Daten werden überschrieben. Aus deaktiviert beides.",
                "Wähle den Spielstand, der beim nächsten Start geladen wird. Ist er leer oder ungültig, erscheint die normale Auswahl. Aus behält die manuelle Auswahl.",
                "Überspringt das Intro-Video und die Wartezeit auf dem Titelbildschirm. Story-Szenen und Ergebnisse bleiben unverändert.",
                "Legt fest, wie oft regelmäßig gespeichert wird. Das Spiel speichert auch bei der Rückkehr zum Planeten und beim normalen Beenden.",
                "Beendet diesen Versuch und geht zur normalen Bewertung. Wurde das Ziel nicht erreicht, kann das als Fehlschlag zählen.",
                "Schließt Katamari Plus und kehrt zum vorherigen Bildschirm zurück. Optionen werden automatisch gespeichert."},
            On="An",Off="Aus",Original="Original",Slot="Spielstand {0}",Seconds="{0} s",
            PauseTip="Öffne Katamari Plus, um Funktionen einzustellen oder das Level zu beenden.",
            ControlTip="Einfache Steuerung: linker Stick bewegt · rechter Stick seitwärts dreht · rechter Stick nach unten lädt den Sprint auf.",
            Confirm="Level jetzt beenden?",Demo="Demo-Musik"
        },
        new PlusLang {
            Code="it",
            Labels=new[]{"Tempo infinito","Jukebox","Salvataggio auto","Caricamento auto","Salta intro","Intervallo salvataggio","Termina livello","Indietro"},
            Tips=new[]{
                "Ferma l'orologio. Per terminare, torna alla pausa e apri Katamari Plus > Termina livello.",
                "Sostituisce solo la musica del livello. Fuori dal livello, la scelta vale per il successivo. Originale ripristina la traccia normale.",
                "Scegli lo slot che riceve i progressi alla chiusura e nei salvataggi periodici. I suoi dati verranno sovrascritti. Disattivato li disattiva entrambi.",
                "Scegli il salvataggio da aprire al prossimo avvio. Se è vuoto o non valido, appare la selezione normale. Disattivato mantiene la selezione manuale.",
                "Salta il filmato iniziale e l'attesa nella schermata del titolo. Le scene della storia e i risultati restano normali.",
                "Imposta la frequenza del salvataggio periodico. Il gioco prova a salvare anche al ritorno sul pianeta e alla chiusura normale.",
                "Termina questo tentativo e passa alla valutazione normale. Se l'obiettivo non è stato raggiunto, il risultato può essere un fallimento.",
                "Chiude Katamari Plus e torna alla schermata precedente. Le opzioni vengono salvate automaticamente."},
            On="Attivo",Off="Disattivato",Original="Originale",Slot="Salvataggio {0}",Seconds="{0} s",
            PauseTip="Apri Katamari Plus per regolare le funzioni o terminare il livello.",
            ControlTip="Comandi semplici: levetta sinistra muove · levetta destra di lato gira · levetta destra in basso carica lo scatto.",
            Confirm="Terminare il livello adesso?",Demo="Musica della demo"
        },
        new PlusLang {
            Code="es",
            Labels=new[]{"Tiempo infinito","Jukebox","Guardado automático","Carga automática","Saltar intro","Intervalo de guardado","Terminar fase","Volver"},
            Tips=new[]{
                "Mantiene el reloj detenido. Para terminar, vuelve a la pausa y abre Katamari Plus > Terminar fase.",
                "Sustituye solo la música de la fase. Fuera de ella, la elección se aplica a la siguiente. Original restaura la pista normal.",
                "Elige la ranura que recibe el progreso al cerrar y en los guardados periódicos. Sus datos se sobrescribirán. Desactivado desactiva ambos.",
                "Elige la partida que se abrirá en el próximo inicio. Si está vacía o no es válida, aparece la selección normal. Desactivado mantiene la selección manual.",
                "Salta el vídeo de introducción y la espera en la pantalla de título. Las escenas de la historia y los resultados siguen igual.",
                "Define cada cuánto se guarda periódicamente. El juego también intenta guardar al volver al planeta y al cerrar con normalidad.",
                "Termina este intento y pasa a la evaluación normal. Si no se alcanzó la meta, el resultado puede ser un fracaso.",
                "Cierra Katamari Plus y vuelve a la pantalla anterior. Las opciones se guardan automáticamente."},
            On="Activado",Off="Desactivado",Original="Original",Slot="Partida {0}",Seconds="{0} s",
            PauseTip="Abre Katamari Plus para ajustar las funciones o terminar la fase.",
            ControlTip="Control simple: stick izquierdo mueve · stick derecho a los lados gira · stick derecho hacia abajo carga el acelerón.",
            Confirm="¿Terminar la fase ahora?",Demo="Música de la demo"
        },
        new PlusLang {
            Code="pt",
            Labels=new[]{"Tempo infinito","Jukebox","Salvamento automático","Carregamento automático","Pular abertura","Intervalo de salvamento","Encerrar fase","Voltar"},
            Tips=new[]{
                "Mantém o relógio parado. Para encerrar, volte à pausa e abra Katamari Plus > Encerrar fase.",
                "Substitui apenas a música da fase. Fora dela, a escolha vale para a próxima fase. Original restaura a trilha normal.",
                "Escolha o slot que receberá o progresso ao fechar e nos salvamentos periódicos. Os dados desse slot serão substituídos. Desligado desativa ambos.",
                "Escolha o save para abrir na próxima inicialização. Se estiver vazio ou inválido, o jogo mostra a seleção normal. Desligado mantém a seleção manual.",
                "Pula a abertura e a espera na tela de título. As cenas da história e os resultados continuam normais.",
                "Define a frequência do salvamento periódico. Ao voltar ao Planeta Natal e fechar normalmente, o jogo também tenta salvar.",
                "Encerra esta tentativa e segue para a avaliação normal. Se a meta não foi atingida, o resultado pode ser uma falha.",
                "Fecha o Katamari Plus e volta à tela anterior. As opções são salvas automaticamente."},
            On="Ligado",Off="Desligado",Original="Original",Slot="Save {0}",Seconds="{0} s",
            PauseTip="Abra Katamari Plus para ajustar os recursos ou encerrar a fase.",
            ControlTip="Controle simples: esquerdo move · direito para os lados gira · direito para baixo carrega a arrancada.",
            Confirm="Encerrar a fase agora?",Demo="Música da demonstração",
            Continue="Continuar",Home="Voltar ao menu"
        }
    };

    // Rótulos fixos em inglês para o registro (qol.log).
    static readonly string[] logLabels={"Infinite time","Jukebox","Autosave","Autoload","Skip opening","Autosave interval","End stage","Back"};

    static int detectedLang=-1;

    static PlusLang T { get { return plusLangs[LangIndex()]; } }

    static int LangIndex()
    {
        string forced=settings.Language;
        if(!string.IsNullOrEmpty(forced) && forced!="auto")
            for(int i=0;i<plusLangs.Length;i++)if(plusLangs[i].Code==forced)return i;
        if(detectedLang>=0)return detectedLang;
        if(work==null)return LangEn;
        int game=(int)work.language;
        if(game<0 || game>5)game=LangEn; // chinês tradicional: o jogo usa os textos em inglês na Steam
        if(game!=LangEn){detectedLang=game;Log("Plus language: "+plusLangs[game].Code);return game;}
        if(work.localKingText==null)return LangEn; // tabela ainda não carregada; tenta de novo depois
        try
        {
            // UI_SYS_289 é "Continue" no jogo original e "Continuar" com a tradução PT-BR.
            string probe=work.localKingText.GetLocaliseText("UI_SYS_289",LangEn);
            detectedLang=probe=="Continuar"?LangPt:LangEn;
        }
        catch(Exception){detectedLang=LangEn;}
        Log("Plus language: "+plusLangs[detectedLang].Code);
        return detectedLang;
    }

    static bool IsPortuguese { get { return LangIndex()==LangPt; } }

    static string MusicName(int index)
    {
        return index==musicNames.Length-1?T.Demo:musicNames[index];
    }
}
